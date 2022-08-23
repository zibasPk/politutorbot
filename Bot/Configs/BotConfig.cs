using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfig
{
    public string BotToken = "";
    public int BotLogLevel;
    public int UserTimeOut;
    public int TutorLockHours;

    public BotConfig()
    {
        // Default log level 3: information
        BotLogLevel = 3;
    }

    public static void InitializeConfig()
    {
        try
        {
            Log.Verbose("Loading BotConfig from json file");
            var text = File.ReadAllText(Paths.BotConfig);
            var botConfig = JsonConvert.DeserializeObject<BotConfig>(text);
            GlobalConfig.BotConfig = botConfig;
        }
        catch(FileNotFoundException e)
        {
            Log.Warning("No BotConfig json found, generating template file in Bot/Data/");
            GenerateEmptyConfig();
        }

    }

    private static void GenerateEmptyConfig()
    {
        GlobalConfig.BotConfig = new BotConfig();
        var x = JsonConvert.SerializeObject(GlobalConfig.BotConfig);
        File.WriteAllText(Paths.BotConfig, x);
    }
}