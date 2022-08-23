﻿using Bot.configs;
using Bot.Database;
using Bot.WebApi;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Bot;

internal static class Program
{
    private static async Task Main()
    {
       // Global logger configuration
        var levelSwitch = new LoggingLevelSwitch();
        var logConfig = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch);
        levelSwitch.MinimumLevel = LogEventLevel.Information;
        Log.Logger = logConfig.WriteTo.Console()
            .WriteTo.File("logs/botlog.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        levelSwitch.MinimumLevel = GlobalConfig.GetBotLogLevel();
        Log.Information("Bot logger started on {level} level", levelSwitch.MinimumLevel);
        
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
        AsyncResponseHandler.Init(botClient);
        
        Log.Information("Start listening for " + me.Username);
        if (GlobalConfig.WebConfig!.IsActive)
            WebServer.Init();
        Console.ReadLine();
        // Send cancellation request to stop bot
        cts.Cancel();
    }
}