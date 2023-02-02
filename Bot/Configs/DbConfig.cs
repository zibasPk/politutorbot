using Bot.Constants;

namespace Bot.configs;

public class DbConfig : Config
{
    public string Host = "";
    public string Port = "";
    public string User = "";
    public string Password = "";
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