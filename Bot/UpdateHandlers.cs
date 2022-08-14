using System.Collections;
using Bot.Enums;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public static class UpdateHandlers
{
    private static Dictionary<long, Conversation> _idToConversation = new();

    public static Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Log.Error(errorMessage);
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        // Check the received message type and start a handler
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => UnknownUpdateHandlerAsync(botClient, update),
            UpdateType.CallbackQuery => BotOnCallbackReceived(botClient, update),
            UpdateType.InlineQuery => UnknownUpdateHandlerAsync(botClient, update),
            UpdateType.ChosenInlineResult => UnknownUpdateHandlerAsync(botClient, update),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await PollingErrorHandler(botClient, exception, cancellationToken);
        }
    }

    private static async Task BotOnCallbackReceived(ITelegramBotClient botClient, Update update)
    {
        //throw new NotImplementedException();
    }


    private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
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

        static async Task<Message> SendSchoolKeyboard(ITelegramBotClient botClient, Message message)
        {
            // Change conversation state
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
            conversation!.State = UserState.School;

            Log.Debug("Sending School inline keyboard to chat: {id}.", message.Chat.Id);
            // Show typing action to client
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            // Simulate longer running task
            // await Task.Delay(500)

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton[] { "3I", "ICAT" },
                    new KeyboardButton[] { "AUIC", "Design" }
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Scegli la tua scuola",
                replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> SendCourseKeyboard(ITelegramBotClient botClient, Message message, string school)
        {
            ReplyKeyboardMarkup? replyKeyboardMarkup = Navigator.GenerateCourseKeyboard(school);

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

        static async Task<Message> SendYearKeyboard(ITelegramBotClient botClient, Message message, string course)
        {
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
            // Check course validity
            if (!Navigator.IsCourseInSchool(course, conversation!.School!))
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

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton[] { "Y1", "Y2" },
                    new KeyboardButton[] { "Y3" },
                    new KeyboardButton[] {"indietro"}
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Scegli il tuo anno",
                replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> SendExamKeyboard(ITelegramBotClient botClient, Message message, string year)
        {
            _idToConversation.TryGetValue(message.From!.Id, out var conversation);
            // Check course validity
            var replyKeyboardMarkup = Navigator.GenerateSubjectKeyboard(conversation!.Course!, year);
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

        static async Task<Message> SendEcho(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: message.Text!);
        }
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Log.Information("Unknown update type: {type}", update.Type);
        return Task.CompletedTask;
    }
}