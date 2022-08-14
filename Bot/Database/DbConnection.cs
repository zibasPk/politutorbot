using Bot.configs;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database;

public static class DbConnection
{
    private static MySqlConnection? _mySqlConnection;

    /// <summary>
    /// Opens connection with MySqlDatabase
    /// </summary>
    /// <returns>Saved connection; If none is present returns a new connection</returns>
    public static MySqlConnection GetMySqlConnection()
    {
        if (_mySqlConnection != null)
            return _mySqlConnection;

        _mySqlConnection = new MySqlConnection(GlobalConfig.DbConfig!.GetConnectionString());
        _mySqlConnection.Open();
        Log.Information($"MySQL version : {_mySqlConnection.ServerVersion}");
        return _mySqlConnection;
    }
}