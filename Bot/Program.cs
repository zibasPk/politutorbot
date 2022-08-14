using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Bot;

internal static class Program
{
    private static async Task Main(String[] args)
    {
        var levelSwitch = new LoggingLevelSwitch();
        var logConfig = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch);
        levelSwitch.MinimumLevel = LogEventLevel.Information;
        Log.Logger = logConfig.WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        // initialize config Classes
        GlobalConfig.InitConfigs();
        levelSwitch.MinimumLevel = GlobalConfig.GetLogLevel();
        Log.Information("Logger started on {level} level", levelSwitch.MinimumLevel);
        
        //Db connection initialization
        DbConnection.GetMySqlConnection();
        
        // Bot initialization
        var botClient = new TelegramBotClient(GlobalConfig.BotConfig!.BotToken);
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