using Bot.configs;
using MySql.Data.MySqlClient;

namespace Bot.Database.Dao;

public class AuthenticationDAO : DAO
{
  public AuthenticationDAO(MySqlConnection connection): base(connection)
  {
  }

  /// <summary>
  /// Check if a token is valid.
  /// </summary>
  /// <param name="token">Token to check.</param>
  /// <returns>true if token is valid, false if token doesn't exist or is expired</returns>
  public bool CheckToken(string token)
  {
    Connection.Open();
    const string query = "SELECT * FROM auth_token WHERE token = @token AND " +
                         "created_at > DATE_SUB(NOW(), INTERVAL @days DAY)";
    
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@token", token);
      command.Parameters.AddWithValue("@days", GlobalConfig.WebConfig!.TokenValidityDays);
      var reader = command.ExecuteReader();
      var result = reader.HasRows;
      Connection.Close();
      return result;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Insert a new token into the db. 
  /// </summary>
  /// <param name="tokenString">Hashed token to insert.</param>
  public void InsertToken(string tokenString)
  {
    Connection.Open();
    // Done here because as a trigger it would need a key value in the where clause
    const string cleanupQuery = "DELETE FROM auth_token WHERE created_at < " +
                                "DATE_SUB(NOW(), INTERVAL @days DAY)";
    try
    {
      var command = new MySqlCommand(cleanupQuery, Connection);
      command.Parameters.AddWithValue("@days", GlobalConfig.WebConfig!.TokenValidityDays);
      command.Prepare();
      command.ExecuteNonQuery();
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
    
    const string query = "INSERT INTO auth_token (token) VALUES (@token)";
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@token", tokenString);
      command.Prepare();
      command.ExecuteNonQuery();
      Connection.Close();
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  public bool VerifyMail(string emailStr)
  {
    Connection.Open();
    const string query = "SELECT * FROM authorized_email WHERE email = @email";
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@email", emailStr);
      command.Prepare();
      var reader = command.ExecuteReader();
      if (reader.HasRows)
      {
        Connection.Close();
        return true;
      }
      Connection.Close();
      return false;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }
}