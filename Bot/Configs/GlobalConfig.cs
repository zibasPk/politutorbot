using Serilog.Events;

namespace Bot.configs;

public static class GlobalConfig
{
    public static BotConfig? BotConfig { get; set; }
    public static DbConfig? DbConfig { get; set; }

    public static void InitConfigs()
    {
        BotConfig.InitializeConfig();
        DbConfig.InitializeConfig();
    }

    public static LogEventLevel GetLogLevel()
    {
        if (BotConfig == null)
            return LogEventLevel.Information;

        return BotConfig.LogLevel switch
        {
            1 => LogEventLevel.Verbose,
            2 => LogEventLevel.Debug,
            3 => LogEventLevel.Information,
            4 => LogEventLevel.Warning,
            5 => LogEventLevel.Error,
            6 => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
    }
}