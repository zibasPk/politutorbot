using Bot.Constants;
using Newtonsoft.Json;

namespace Bot.configs;

public class DbConfig : Config
{   
    [JsonProperty(Required = Required.Always)]
    public string Host = "";
    [JsonProperty(Required = Required.Always)]
    public string Port = "";
    [JsonProperty(Required = Required.Always)]
    public string User = "";
    [JsonProperty(Required = Required.Always)]
    public string Password = "";
    [JsonProperty(Required = Required.Always)]
    public string DbName = "";

    public DbConfig()
    {
        FilePath = Paths.DbConfig;
    }

    /// <summary>
    /// Generates a connection string for a MySql database.
    /// </summary>
    /// <returns>Generated connection string.</returns>
    public string GetConnectionString()
    {
        return @"server=" + Host + "; port=" + Port +
               "; userid=" + User + "; password=" + Password + "; database=" + DbName + "; convert zero datetime=True";
    }
}