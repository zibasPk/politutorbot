using Serilog;
using Serilog.Core;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Bot;

internal static class Program
{
    public static Dictionary<long, Conversation> dict = new Dictionary<long, Conversation>();

    private static async Task Main(String[] args)
    {
        var levelSwitch = new LoggingLevelSwitch();
        LoggerConfiguration logConfig = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch);
        levelSwitch.MinimumLevel = LogEventLevel.Information;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-d":
                case "--debug":
                    levelSwitch.MinimumLevel = LogEventLevel.Debug;
                    break;
                default:
                    Console.Out.WriteLine("Unknown command line argument " + args[i]);
                    break;
            }
        }

        Log.Logger = logConfig.WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information("Logger started on {level} level", levelSwitch.MinimumLevel);


        TelegramBotClient botClient = new TelegramBotClient(BotConfiguration.BotToken);
        var me = await botClient.GetMeAsync();

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        botClient.StartReceiving(
            updateHandler: UpdateHandlers.HandleUpdateAsync,
            pollingErrorHandler: UpdateHandlers.PollingErrorHandler,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Log.Information("Start listening for " + me.Username);
        Console.ReadLine();
        // Send cancellation request to stop bot
        cts.Cancel();
    }
}