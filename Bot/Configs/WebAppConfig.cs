using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class WebAppConfig
{
    public bool IsActive;
    public int Port;
    public int WebLogLevel;
    public string LoginLink;

    public WebAppConfig()
    {
        // default start option
        IsActive = false;
        // default log level 3: information
        WebLogLevel = 3;
        // default port
        Port = 3000;
        // default link
        LoginLink = "www.example.com";
    }

    public static void InitializeConfig()
    {
        try
        {
            Log.Debug("Loading web app config from json file");
            var text = File.ReadAllText(Paths.WebAppConfig);
            var webAppConfig = JsonConvert.DeserializeObject<WebAppConfig>(text);
            GlobalConfig.WebConfig = webAppConfig;
        }
        catch(FileNotFoundException e)
        {
            Log.Warning("No web app config json found, generating template file in Bot/Data/");
            GenerateEmptyConfig();
        }

    }

    private static void GenerateEmptyConfig()
    {
        GlobalConfig.WebConfig = new WebAppConfig();
        var x = JsonConvert.SerializeObject(GlobalConfig.WebConfig);
        File.WriteAllText(Paths.WebAppConfig, x);
    }
}
