using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

/// <summary>
/// A class containing configuration data for the web app attached to the telegram bot.
/// </summary>
public class WebAppConfig : Config
{
    public string Url;
    public int Port;
    public int WebLogLevel;
    public string AuthUsr;
    public string AuthPsw;
    public string AllowedCors;
    public WebAppConfig()
    {
        // default log level 3: information
        WebLogLevel = 3;
        // default port
        Url = "https://localhost";
        Port = 5000;
        // default Api authentication
        AuthUsr = "admin";
        AuthPsw = "admin";
        AllowedCors = "";
        FilePath = Paths.WebAppConfig;
    }
}
