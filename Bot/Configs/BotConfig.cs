using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

/// <summary>
/// Contains config information for telegram bot back end.
/// </summary>
public class BotConfig: Config
{
    public string BotToken;
    public int BotLogLevel;
    public int UserTimeOut;
    public int TutorLockHours;
    public bool HasOnlineAuth;
    public string AuthLink;
    public int MaxTutoringDuration;

    public BotConfig()
    {
        // Default log level 3: information
        BotToken = "";
        BotLogLevel = 3;
        UserTimeOut = 120000;
        TutorLockHours = 24;
        HasOnlineAuth = false;
        MaxTutoringDuration = 150;
        // default link for online authentication
        AuthLink = "www.example.com";
        
        // initialize file path
        FilePath = Paths.BotConfig;
    }
    
}