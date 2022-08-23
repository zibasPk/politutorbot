using System.Collections;
using Bot.Database;
using Bot.Database.Dao;
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
        // Check the received message type and give it to a handler
        var handler = update.Type switch
        {
            UpdateType.Message => MessageHandlers.HandleMessage(botClient, update.Message!),
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

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Log.Information("Unknown update type: {type}", update.Type);
        return Task.CompletedTask;
    }
}