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
        // Default log level 3: information
        BotToken = "";
        BotLogLevel = 3;
        UserTimeOut = 120000;
        TutorLockHours = 24;
        HasOnlineAuth = false;
        MaxTutoringDuration = 150;
        ShownTutorsInList = 5;
        ShownTutorsInOFAList = 8;
        // default link for online authentication
        AuthLink = "www.example.com";
        
        // initialize file path
        FilePath = Paths.BotConfig;
    }
    
}