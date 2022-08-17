using System.Text.RegularExpressions;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Enums;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public static class MessageHandlers
{
    private static readonly Dictionary<long, Conversation> IdToConversation = new();

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
        Log.Information("Received message '{text}' in chat {chatId}", message.Text, chatId);
        if (!IdToConversation.TryGetValue(userId, out var conversation))
        {
            conversation = new Conversation();
            IdToConversation.Add(userId, conversation);
        }

        var command = message.Text;
        if (command == "indietro")
        {
            conversation.GoToPreviousState();
            conversation.GoToPreviousState();
            command = conversation.GetCurrentTopic();
        }

        var action = conversation!.State switch
        {
            UserState.Start => SendSchoolKeyboard(botClient, message),
            UserState.School => SendCourseKeyboard(botClient, message, command!),
            UserState.Course => SendYearKeyboard(botClient, message, command!),
            UserState.Year => SendExamKeyboard(botClient, message, command!),
            UserState.Exam => SendSaveUserData(botClient, message, command!),
            UserState.Link => ReadStudentNumber(botClient, message, command!),
            UserState.ReLink => ReadYesOrNo(botClient, message, command!),
            _ => SendEcho(botClient, message)
        };

        await action;
    }

    private static async Task<Message> SendSchoolKeyboard(ITelegramBotClient botClient, Message message)
    {
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);

        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (!Monitor.TryEnter(conversation!.ConvLock))
        {
            Log.Debug("locked conv rip");
            return await SendEcho(botClient, message);
        }

        var replyKeyboardMarkup = KeyboardGenerator.GenerateSchoolKeyboard();
        if (replyKeyboardMarkup == null)
        {
            Log.Error("No schools found!.");
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
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);

        var replyKeyboardMarkup = KeyboardGenerator.GenerateCourseKeyboard(school);
        if (school != "ICAT")
        {
            Log.Debug("Unavailable school {school} chosen in chat {id}.", school, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Il servizio è momentaneamente attivo solo per la scuola ICAT");
        }

        // Check if school is valid
        if (replyKeyboardMarkup == null)
        {
            Log.Debug("Invalid school {school} chosen in chat {id}.", school, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci una scuola valida");
        }

        // Show typing action to client
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
        // Change conversation state to Course and save chosen school
        conversation!.State = UserState.Course;
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
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);

        // Check course validity
        var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
        if (!courseService.IsCourseInSchool(course, conversation!.School!))
        {
            Log.Debug("Invalid {course} chosen in chat {id}.", course, message.Chat.Id);
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
        // Simulate longer running task
        // await Task.Delay(500);

        //TODO: generate keyboard dynamically and check if null
        var replyKeyboardMarkup = KeyboardGenerator.GenerateYearKeyboard();

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Scegli il tuo anno",
            replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> SendExamKeyboard(ITelegramBotClient botClient, Message message, string year)
    {
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);
        // Check course validity
        var replyKeyboardMarkup = KeyboardGenerator.GenerateSubjectKeyboard(conversation!.Course!, year);
        if (replyKeyboardMarkup == null)
        {
            Log.Debug("Invalid {year} chosen in chat {id}.", year, message.Chat.Id);
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
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);
        var connection = DbConnection.GetMySqlConnection();
        var examService = new ExamDAO(connection);
        if (!examService.IsExamInCourse(exam, conversation!.Course!, conversation.Year!))
        {
            Log.Debug("Invalid Exam {exam} chosen in chat {id}.", exam, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci una materia valida");
        }

        conversation.State = UserState.Link;
        var userService = new UserDAO(connection);
        var userId = message.From.Id;
        var studentNumber = userService.FindUserStudentNumber(userId);
        if (studentNumber == null)
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci il tuo codice matricola:",
                replyMarkup: new ReplyKeyboardRemove());

        conversation.StudentNumber = studentNumber.Value;
        conversation.State = UserState.ReLink;
        // Release lock on conversation
        Monitor.Exit(conversation.ConvLock);

        Log.Debug("User {userId} already linked to person code {studentNumber}.", userId, studentNumber);
        var replyKeyboardMarkup = KeyboardGenerator.GenerateYesOrNoKeyboard();
        return await
            botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: $"Il tuo id telegram è già associato al codice matricola {studentNumber}.\n" +
                      "Vuoi reinserire?",
                replyMarkup: replyKeyboardMarkup);
    }

    private static async Task<Message> ReadYesOrNo(ITelegramBotClient botClient, Message message, string command)
    {
        var userId = message.From!.Id;
        IdToConversation.TryGetValue(userId, out var conversation);

        var studentNumber = conversation!.StudentNumber;
        switch (command)
        {
            case "Sì":
            case "Si":
            case "si":
            case "SI":
                // Check if conversation has been reset or is locked by a reset if not acquire lock
                if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
                    return await SendEcho(botClient, message);
                Log.Debug("User {userId} has chosen to delete association with person code {studentNumber}"
                    , userId, studentNumber);
                var userService = new UserDAO(DbConnection.GetMySqlConnection());
                userService.RemoveUser(userId);
                conversation.StudentNumber = 0;
                conversation.State = UserState.Link;
                // Release lock from conversation
                Monitor.Exit(conversation.ConvLock);
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: "Associazione rimossa. \nReinserisci il tuo codice matricola:",
                    replyMarkup: new ReplyKeyboardRemove());
            case "No":
            case "no":
            case "NO":
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: "Codice persona confermato");
            default:
                Log.Debug("Invalid {studentNumber} chosen in chat {id}.", studentNumber, message.Chat.Id);
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: "Inserisci una risposta valida");
        }
    }

    private static async Task<Message> ReadStudentNumber(ITelegramBotClient botClient, Message message,
        string studentNumberStr)
    {
        IdToConversation.TryGetValue(message.From!.Id, out var conversation);
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
            return await SendEcho(botClient, message);
        if (!Regex.IsMatch(studentNumberStr, "^[1-9][0-9]{5}"))
        {
            Log.Debug("Invalid student number {studentNumber} typed in chat {id}.", studentNumberStr, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Inserisci un codice matricola valido");
        }

        var studentNumber = int.Parse(studentNumberStr);
        var userId = message.From!.Id;
        var userService = new UserDAO(DbConnection.GetMySqlConnection());
        userService.SaveUserLink(userId, studentNumber);
        //TODO: ask what's next

        // Release lock from conversation
        Monitor.Exit(conversation.ConvLock);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Codice matricola salvato!");
    }

    private static async Task<Message> SendEcho(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: message.Text!);
    }
}