using Bot.Constants;
using Newtonsoft.Json;

namespace Bot.configs;

/// <summary>
/// Contains config information for telegram bot back end.
/// </summary>
public class BotConfig: Config
{
    [JsonProperty(Required = Required.Always)]
    public string BotToken;
    public int BotLogLevel;
    public int UserTimeOut;
    public int TutorLockHours;
    public bool HasOnlineAuth;
    public string AuthLink;
    public int MaxTutoringDuration;
    public int ShownTutorsInList;
    public int ShownTutorsInOFAList;

    public BotConfig()
    {
        BotToken = "";
        BotLogLevel = 3;  // Default log level 3: information
        UserTimeOut = 120000;
        TutorLockHours = 24;
        HasOnlineAuth = false;
        MaxTutoringDuration = 150;
        ShownTutorsInList = 5;
        ShownTutorsInOFAList = 8; 
        AuthLink = "www.example.com"; // default link for online authentication on bot
        
        // initialize file path
        FilePath = Paths.BotConfig;
    }
    
}