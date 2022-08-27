using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Enums;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Bot.AsyncResponseHandler;

namespace Bot;

public static class MessageHandlers
{
    private static readonly Dictionary<long, Conversation> UserIdToConversation = new();

    public static async void OnApiCall(int userId)
    {
        UserIdToConversation.TryGetValue(userId, out var conversation);
        if (conversation == null)
        {
            Log.Warning("Received api call from user: {userId} with no active conversation", userId);
            return;
        }

        if (!conversation.WaitingForApiCall)
        {
            Log.Warning("Received api call from conversation with user: {userId} " +
                        "that wasn't waiting for an api call", userId);
            return;
        }

        Log.Debug("Received api call from user: {userId}", userId);
        lock (conversation.ConvLock)
        {
            conversation.WaitingForApiCall = false;
            // TODO: change db and application logic for person code
        }

        await SendMessage(conversation.ChatId, "Identità confermata con successo");
    }

    public static async Task HandleMessage(ITelegramBotClient botClient, Message message)
    {
        var chatId = message.Chat.Id;
        if (string.IsNullOrEmpty(message.Text))
        {
            Log.Warning("Received message with no text in chat {chatId}", chatId);
            return;
        }

        if (message.From == null)
        {
            Log.Warning("Received message '{text}' in chat {chatId} with no attached User", message.Text, chatId);
            return;
        }

        var userId = message.From.Id;
        Log.Information("Received message '{text}' by user {userId}", message.Text, userId);

        // Check if user is already locked for finalizing a tutor request
        // TODO: non fare il controllo su db ogni messaggio?
        var userService = new UserDAO(DbConnection.GetMySqlConnection());
        if (userService.IsUserLocked(userId, GlobalConfig.BotConfig!.TutorLockHours))
        {
            await botClient.SendTextMessageAsync(chatId: chatId,
                text: $"Sei bloccato dal fare nuove richieste per {GlobalConfig.BotConfig.TutorLockHours} " +
                      $"ore dalla tua precedente richiesta " +
                      $"o fino a che la segreteria non l'avrà elaborata.",
                replyMarkup: new ReplyKeyboardRemove());
            return;
        }

        if (!UserIdToConversation.TryGetValue(userId, out var conversation))
        {
            conversation = new Conversation(chatId);
            UserIdToConversation.Add(userId, conversation);
        }

        var command = message.Text;
        if (command == "indietro")
        {
            if (conversation.WaitingForApiCall)
                conversation.WaitingForApiCall = false;
            conversation.GoToPreviousState();
            conversation.GoToPreviousState();
            command = conversation.GetCurrentTopic();
        }

        var action = conversation.State switch
        {
            UserState.Start => SendSchoolKeyboard(botClient, message),
            UserState.School => SendCourseKeyboard(botClient, message, command!),
            UserState.Course => SendYearKeyboard(botClient, message, command!),
            UserState.Year => SendExamKeyboard(botClient, message, command!),
            UserState.Exam => SendSaveUserData(botClient, message, command!),
            UserState.Link => ReadStudentNumber(botClient, message, command!),
            UserState.ReLink => ReadYesOrNo(botClient, message, command!),
            UserState.Tutor => ReadTutor(botClient, message, command!),
            _ => SendEcho(botClient, message)
        };

        await action;
    }

    private static async Task<Message> SendSchoolKeyboard(ITelegramBotClient botClient, Message message)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);

        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (!Monitor.TryEnter(conversation!.ConvLock))
        {
            Log.Debug("locked conv rip");
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await SendEcho(botClient, message);
        }

        var replyKeyboardMarkup = KeyboardGenerator.SchoolKeyboard();
        if (replyKeyboardMarkup == null)
        {
            Log.Error("No schools found!.");
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Errore Interno al Bot");
        }

        // Change conversation state
        conversation.State = UserState.School;

        // Release lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("Sending School inline keyboard to chat: {id}.", message.Chat.Id);
        // Show typing action to client
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Scegli la tua scuola",
            replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> SendCourseKeyboard(ITelegramBotClient botClient, Message message, string school)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);

        var replyKeyboardMarkup = KeyboardGenerator.CourseKeyboard(school);
        if (school != "ICAT")
        {
            Log.Debug("Unavailable school {school} chosen in chat {id}.", school, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Il servizio è momentaneamente attivo solo per la scuola ICAT");
        }

        // Check if school is valid
        if (replyKeyboardMarkup == null)
        {
            Log.Debug("Invalid school {school} chosen in chat {id}.", school, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci una scuola valida");
        }

        // Show typing action to client
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
        // Change conversation state to Course and save chosen school
        conversation.State = UserState.Course;
        conversation.School = school;

        // Release lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("Sending Course inline keyboard for school {school} to chat: {id}.", school, message.Chat.Id);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Scegli il tuo corso di studi",
            replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> SendYearKeyboard(ITelegramBotClient botClient, Message message, string course)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);

        // Check course validity
        var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
        if (!courseService.IsCourseInSchool(course, conversation.School!))
        {
            Log.Debug("Invalid {course} chosen in chat {id}.", course, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci un corso valido");
        }

        // Change conversation state to Year and save chosen course
        conversation.State = UserState.Year;
        conversation.Course = course;
        // Release Lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("Sending Year inline keyboard to chat: {id}.", message.Chat.Id);
        // Show typing action to client
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        var replyKeyboardMarkup = KeyboardGenerator.YearKeyboard();

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Scegli il tuo anno",
            replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> SendExamKeyboard(ITelegramBotClient botClient, Message message, string year)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);
        // Check course validity
        var replyKeyboardMarkup = KeyboardGenerator.SubjectKeyboard(conversation.Course!, year);
        if (replyKeyboardMarkup == null)
        {
            Log.Debug("Invalid {year} chosen in chat {id}.", year, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci un anno valido");
        }

        // Change conversation state to Subject and save chosen year
        conversation.State = UserState.Exam;
        conversation.Year = year;
        // Release lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("Sending Exam inline keyboard to chat: {id}.", message.Chat.Id);
        // Show typing action to client
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Scegli la materia per cui ti serve un tutoraggio",
            replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> SendSaveUserData(ITelegramBotClient botClient, Message message, string exam)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);
        var connection = DbConnection.GetMySqlConnection();
        var examService = new ExamDAO(connection);
        if (!examService.IsExamInCourse(exam, conversation.Course!, conversation.Year!))
        {
            Log.Debug("Invalid Exam {exam} chosen in chat {id}.", exam, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci una materia valida");
        }

        conversation.Exam = exam;
        conversation.State = UserState.Link;
        var userService = new UserDAO(connection);
        var userId = message.From.Id;
        var studentNumber = userService.FindUserStudentNumber(userId);
        if (studentNumber == null)
        {
            // Check if Online Authentication is active
            if (GlobalConfig.WebConfig!.IsActive)
            {
                conversation.WaitingForApiCall = true;
                Monitor.Exit(conversation.ConvLock);
                var text = "Login Aunica: " + GlobalConfig.WebConfig.LoginLink;
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: KeyboardGenerator.BackKeyboard());
            }

            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci il tuo codice matricola:",
                replyMarkup: new ReplyKeyboardRemove());
        }

        conversation.StudentNumber = studentNumber.Value;
        conversation.State = UserState.ReLink;
        // Release lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("User {userId} already linked to person code {studentNumber}.", userId, studentNumber);
        var replyKeyboardMarkup = KeyboardGenerator.YesOrNoKeyboard();
        return await
            botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: $"Il tuo id telegram è già associato al codice matricola {studentNumber}.\n" +
                      "Vuoi reinserire?",
                replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> ReadYesOrNo(ITelegramBotClient botClient, Message message, string command)
    {
        var userId = message.From!.Id;
        UserIdToConversation.TryGetValue(userId, out var conversation);

        var studentNumber = conversation!.StudentNumber;
        switch (command)
        {
            case "Sì":
            case "Si":
            case "si":
            case "SI":
                // Check if conversation has been reset or is locked by a reset if not acquire lock
                if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
                    return await SendEcho(botClient, message);
                Log.Debug("User {userId} has chosen to delete association with person code {studentNumber}"
                    , userId, studentNumber);
                var userService = new UserDAO(DbConnection.GetMySqlConnection());
                userService.RemoveUser(userId);
                conversation.StudentNumber = 0;
                conversation.State = UserState.Link;
                // Release lock from conversation

                // Check if Online Authentication is active
                if (!GlobalConfig.WebConfig!.IsActive)
                {
                    Monitor.Exit(conversation.ConvLock);
                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                        text: "Associazione rimossa. \nReinserisci il tuo codice matricola:",
                        replyMarkup: new ReplyKeyboardRemove());
                }

                conversation.WaitingForApiCall = true;
                Monitor.Exit(conversation.ConvLock);
                var text = "Login Aunica: " + GlobalConfig.WebConfig.LoginLink;
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: KeyboardGenerator.BackKeyboard());
            case "No":
            case "no":
            case "NO":
                // Check if conversation has been reset or is locked by a reset if not acquire lock
                if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
                    return await SendEcho(botClient, message);
                conversation.State = UserState.Tutor;
                // Release lock from conversation
                Monitor.Exit(conversation.ConvLock);
                return await SendTutorsKeyboard(botClient, message);
            default:
                Log.Debug("Invalid {studentNumber} chosen in chat {id}.", studentNumber, message.Chat.Id);
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: "Inserisci una risposta valida");
        }
    }

    private static async Task<Message> ReadStudentNumber(ITelegramBotClient botClient, Message message,
        string studentNumberStr)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
        {
            Log.Debug("user {id} tried to access a locked conversation", message.From.Id);
            return await SendEcho(botClient, message);
        }

        // Check if Online Authentication is active
        if (conversation.WaitingForApiCall)
        {
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Accedi ad Aunica per continuare.");
        }

        if (!Regex.IsMatch(studentNumberStr, "^[1-9][0-9]{5}$"))
        {
            Log.Debug("Invalid student number {studentNumber} typed in chat {id}.", studentNumberStr, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci un codice matricola valido");
        }

        var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
        if (!studentService.IsStudentEnabled(int.Parse(studentNumberStr)))
        {
            Log.Debug("Invalid student number {studentNumber} typed in chat {id}.", studentNumberStr, message.Chat.Id);
            // Release lock from conversation
            Monitor.Exit(conversation.ConvLock);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Spiacente non sei abilitato ai tutoraggi peer to peer.");
        }

        var studentNumber = int.Parse(studentNumberStr);
        var userId = message.From!.Id;
        var userService = new UserDAO(DbConnection.GetMySqlConnection());
        userService.SaveUserLink(userId, studentNumber);
        conversation.State = UserState.Tutor;
        // Release lock from conversation
        Monitor.Exit(conversation.ConvLock);
        return await SendTutorsKeyboard(botClient, message);
    }

    private static async Task<Message> SendEcho(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: message.Text!);
    }

    private static async Task<Message> SendTutorsKeyboard(ITelegramBotClient botClient, Message message)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
        {
            Log.Debug("user {id} tried to access a locked conversation", message.From.Id);
            return await SendEcho(botClient, message);
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        var tutors = tutorService.FindTutorsForExam(conversation.Exam!, GlobalConfig.BotConfig!.TutorLockHours);
        var keyboardMarkup = KeyboardGenerator.TutorKeyboard(tutors);
        var tutorsTexts = tutors.Select(x => "nome: " + x.Name + "\ncorso: " + x.Course + "\n \n").ToList();
        var text = $"Scegli uno dei tutor disponibili per {conversation.Exam}:\n \n";
        foreach (var tutorsText in tutorsTexts)
        {
            text += tutorsText;
        }

        Monitor.Exit(conversation.ConvLock);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: text,
            replyMarkup: keyboardMarkup);
    }

    private static async Task<Message> ReadTutor(ITelegramBotClient botClient, Message message, string tutor)
    {
        var userId = message.From!.Id;
        UserIdToConversation.TryGetValue(userId, out var conversation);
        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        if (!tutorService.IsTutorForExam(tutor, conversation!.Exam!))
        {
            Log.Debug("Invalid {tutor} chosen in chat {id}.", tutor, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci un tutor della lista");
        }

        var userService = new UserDAO(DbConnection.GetMySqlConnection());
        // lock tutor until email arrives
        tutorService.LockTutor(tutor, conversation.Exam!, userId);
        userService.LockUser(userId);
        // TODO: send mail to segreteria
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Tutor selezionato riceverai una mail di conferma dalla segreteria.",
            replyMarkup: new ReplyKeyboardRemove());
    }
}

public static class AsyncResponseHandler
{
    private static ITelegramBotClient? _botClient;

    public static void Init(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public static async Task SendMessage(long chatId, string text)
    {
        if (_botClient == null)
            throw new InvalidOperationException("AsyncResponder class was not initialized. " +
                                                "Call the Init() to initialize static botClient.");
        await _botClient.SendTextMessageAsync(chatId: chatId,
            text: text,
            replyMarkup: new ReplyKeyboardRemove());
    }

    public static async Task SendMessage(long chatId, string text, ITelegramBotClient botClient)
    {
        await botClient.SendTextMessageAsync(chatId: chatId,
            text: text,
            replyMarkup: new ReplyKeyboardRemove());
    }
}