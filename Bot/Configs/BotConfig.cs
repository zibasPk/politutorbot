using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

/// <summary>
/// Contains config information for telegram bot back end.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfig
{
    public string BotToken;
    public int BotLogLevel;
    public int UserTimeOut;
    public int TutorLockHours;

    public BotConfig()
    {
        // Default log level 3: information
        BotToken = "";
        BotLogLevel = 3;
        TutorLockHours = 120000;
        TutorLockHours = 24;
    }

    /// <summary>
    /// Initializes the bot configuration class in <see cref="T:Bot.configs.GlobalConfig" />
    /// by loading the config data from json file. If no file exists it generates an empty one.
    /// </summary>
    public static void InitializeConfig()
    {
        try
        {
            Log.Verbose("Loading BotConfig from json file");
            var text = File.ReadAllText(Paths.BotConfig);
            var botConfig = JsonConvert.DeserializeObject<BotConfig>(text);
            GlobalConfig.BotConfig = botConfig;
        }
        catch (FileNotFoundException)
        {
            Log.Warning("No BotConfig json found, generating template file in Bot/Data/");
            GenerateEmptyConfig();
        }
    }

    /// <summary>
    /// Generates an empty config json.
    /// </summary>
    private static void GenerateEmptyConfig()
    {
        GlobalConfig.BotConfig = new BotConfig();
        var x = JsonConvert.SerializeObject(GlobalConfig.BotConfig);
        File.WriteAllText(Paths.BotConfig, x);
    }
}