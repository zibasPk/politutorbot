using Serilog.Events;

namespace Bot.configs;

public static class GlobalConfig
{
    static GlobalConfig()
    {
        InitConfigs();
    } 
    public static BotConfig? BotConfig { get; set; }
    public static DbConfig? DbConfig { get; set; }
    public static WebAppConfig? WebConfig { get; set; }
    
    public static LogEventLevel GetBotLogLevel()
    {
        if (BotConfig == null)
            return LogEventLevel.Information;

        return BotConfig.BotLogLevel switch
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
    
    public static LogEventLevel GetWebLogLevel()
    {
        if (WebConfig == null)
            return LogEventLevel.Information;

        return WebConfig.WebLogLevel switch
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
    
    private static void InitConfigs()
    {
        BotConfig.InitializeConfig();
        DbConfig.InitializeConfig();
        WebAppConfig.InitializeConfig();
    }
}