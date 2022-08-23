using Bot.Constants;
using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

public class DbConfig
{
    public string Host = "";
    public string Port = "";
    public string User = "";
    public string Password = "";
    public string DbName = ""; 
    
    /// <summary>
    /// Initializes the database configuration class in <see cref="T:Bot.configs.GlobalConfig" />
    /// by loading the config data from json file. If no file exists it generates an empty one.
    /// </summary>
    public static void InitializeConfig()
    {
        try
        {
            Log.Verbose("Loading DbConfig from json file");
            var text = File.ReadAllText(Paths.DbConfig);
            var dbConfig = JsonConvert.DeserializeObject<DbConfig>(text);
            GlobalConfig.DbConfig = dbConfig;
        }
        catch(FileNotFoundException e)
        {
            Log.Warning("No DbConfig json found, generating template file in Bot/Data/");
            GenerateEmptyConfig();
        }

    }

    /// <summary>
    /// Generates an empty config json.
    /// </summary>
    private static void GenerateEmptyConfig()
    {
        GlobalConfig.DbConfig = new DbConfig();
        var x = JsonConvert.SerializeObject(GlobalConfig.DbConfig);
        File.WriteAllText(Paths.DbConfig, x);
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