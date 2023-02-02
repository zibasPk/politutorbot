using Bot.configs;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database;

public static class DbConnection
{
  private static bool _firstConnection = true;

  /// <summary>
  /// Connects to MySql Database. 
  /// </summary>
  /// <returns>Connection to MySQL Server database. </returns>
  public static MySqlConnection GetMySqlConnection()
  {
    var mySqlConnection = new MySqlConnection(GlobalConfig.DbConfig!.GetConnectionString());
    if (!_firstConnection)
      return mySqlConnection;
    mySqlConnection.Open();
    Log.Information($"MySQL version : {mySqlConnection.ServerVersion}");
    _firstConnection = false;
    mySqlConnection.Close();
    return mySqlConnection;
  }
}