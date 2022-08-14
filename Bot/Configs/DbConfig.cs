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

    private static void GenerateEmptyConfig()
    {
        GlobalConfig.DbConfig = new DbConfig();
        var x = JsonConvert.SerializeObject(GlobalConfig.DbConfig);
        File.WriteAllText(Paths.DbConfig, x);
    }

    public string GetConnectionString()
    {
        return @"server=" + Host + "; port=" + Port +
               "; userid=" + User + "; password=" + Password + "; database=" + DbName;
    }
}