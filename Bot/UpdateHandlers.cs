using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot;

public static class UpdateHandlers
{

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
            UpdateType.EditedMessage => DefaultUpdateHandlerAsync(botClient, update),
            UpdateType.CallbackQuery => DefaultUpdateHandlerAsync(botClient, update),
            UpdateType.InlineQuery => DefaultUpdateHandlerAsync(botClient, update),
            UpdateType.ChosenInlineResult => DefaultUpdateHandlerAsync(botClient, update),
            _ => DefaultUpdateHandlerAsync(botClient, update)
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
    
    private static async Task DefaultUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Log.Information("Update type: {type} not implemented.", update.Type);
    }
}