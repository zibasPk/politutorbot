using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot;

internal static class Program {
private static async Task Main(string[] args) {
    LoggerConfiguration logConfig = new LoggerConfiguration().MinimumLevel.Information();
    for (int i = 0; i < args.Length; i++) {
        switch (args[i]) {
            case "-d":
            case "--debug":
                logConfig.MinimumLevel.Debug();
                break;
            default:
                Console.Out.WriteLine("Uknown command line argument");
                break;
        }
    }
    
    Log.Logger = logConfig.WriteTo.Console()
        .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    Log.Information("logger started on info level");
    

    TelegramBotClient botClient = new TelegramBotClient(BotConfiguration.BotToken);
    var me = await botClient.GetMeAsync();
    
    using var cts = new CancellationTokenSource();
    
    var receiverOptions = new ReceiverOptions {
        AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
    };

    botClient.StartReceiving(
        updateHandler: UpdateHandlers.HandleUpdateAsync,
        pollingErrorHandler: UpdateHandlers.HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
    );
    
    Log.Information("Start listening for " + me.Username);
    Console.ReadLine();
    // Send cancellation request to stop bot
    cts.Cancel();
}
}