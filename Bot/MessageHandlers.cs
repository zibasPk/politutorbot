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
    private static Dictionary<long, Conversation> _idToConversation = new();
    
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
        if (!_idToConversation.TryGetValue(userId, out var conversation))
        {
            conversation = new Conversation();
            _idToConversation.Add(userId, conversation);
        }
        
        var command = message.Text.Split(' ')[0];
        if (command == "indietro")
        {
            conversation.GoToPreviousState();
            conversation.GoToPreviousState();
            command = conversation.GetCurrentTopic();
        }

        var action = conversation!.State switch
        {
            UserState.Start => command switch
            {
                "/start" => SendSchoolKeyboard(botClient, message),
                _ => SendEcho(botClient, message)
            },
            UserState.School => SendCourseKeyboard(botClient, message, command),
            UserState.Course => SendYearKeyboard(botClient, message, command),
            UserState.Year => SendExamKeyboard(botClient, message, command),
            _ => SendEcho(botClient, message)
        };

        await action;
    }
    private static async Task<Message> SendSchoolKeyboard(ITelegramBotClient botClient, Message message)
        {
            var replyKeyboardMarkup = KeyboardGenerator.GenerateSchoolKeyboard();
            if (replyKeyboardMarkup == null)
            {
                Log.Error("No schools found!.");
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                    text: "Errore Interno al Bot");
            }
            // Change conversation state
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
            conversation!.State = UserState.School;

            Log.Debug("Sending School inline keyboard to chat: {id}.", message.Chat.Id);
            // Show typing action to client
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            // Simulate longer running task
            // await Task.Delay(500)


            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Scegli la tua scuola",
                replyMarkup: replyKeyboardMarkup);
        }

        private static async Task<Message> SendCourseKeyboard(ITelegramBotClient botClient, Message message, string school)
        {
            ReplyKeyboardMarkup? replyKeyboardMarkup = KeyboardGenerator.GenerateCourseKeyboard(school);

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

            // Change conversation state to Course and save chosen school
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
            conversation!.State = UserState.Course;
            conversation.School = school;
            // Show typing action to client
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            // Simulate longer running task
            // await Task.Delay(500)

            Log.Debug("Sending Course inline keyboard for school {school} to chat: {id}.", school, message.Chat.Id);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Scegli il tuo corso di studi",
                replyMarkup: replyKeyboardMarkup);
        }

        private static async Task<Message> SendYearKeyboard(ITelegramBotClient botClient, Message message, string course)
        {
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
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
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
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

            Log.Debug("Sending Exam inline keyboard to chat: {id}.", message.Chat.Id);
            // Show typing action to client
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Scegli la materia per cui ti serve un tutoraggio",
                replyMarkup: replyKeyboardMarkup);
        }

        private static async Task<Message> SendEcho(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: message.Text!);
        }
}