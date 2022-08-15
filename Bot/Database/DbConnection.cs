using Bot.configs;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database;

public static class DbConnection
{
    private static bool firstConnection = true;
    /// <summary>
    /// Connects to MySql Database 
    /// </summary>
    /// <returns>Connection from connection Pool</returns>
    public static MySqlConnection GetMySqlConnection()
    {
        var mySqlConnection = new MySqlConnection(GlobalConfig.DbConfig!.GetConnectionString());
        if (firstConnection)
        {
            mySqlConnection.Open();
            Log.Information($"MySQL version : {mySqlConnection.ServerVersion}");
            firstConnection = false;
            mySqlConnection.Close();
        }
        return mySqlConnection;
    }
}