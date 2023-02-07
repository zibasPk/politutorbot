using Bot.Constants;
using Newtonsoft.Json;

namespace Bot.configs;

/// <summary>
/// A class containing configuration data for the web app attached to the telegram bot.
/// </summary>
public class WebAppConfig : Config
{
    [JsonProperty(Required = Required.Always)]
    public string Url;
    [JsonProperty(Required = Required.Always)]
    public int Port;
    public int WebLogLevel;
    [JsonProperty(Required = Required.AllowNull)]
    public string AuthUsr;
    [JsonProperty(Required = Required.AllowNull)]
    public string AuthPsw;
    public string AllowedCors;
    [JsonProperty(Required = Required.AllowNull)]
    public string AzureSecret;
    [JsonProperty(Required = Required.AllowNull)]
    public string AzureClientId;
    public string TokenValidityDays;
    public WebAppConfig()
    {
        // default log level 3: information
        WebLogLevel = 3;
        // default port
        Url = "http://+";
        Port = 5000;
        // default Api authentication
        AuthUsr = "admin";
        AuthPsw = "admin";
        AllowedCors = "*";
        FilePath = Paths.WebAppConfig;
        AzureSecret = "";
        AzureClientId = "";
        TokenValidityDays = "30";
    }
}
