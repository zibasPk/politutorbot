using System.Security.Cryptography;
using System.Text;
using Bot.Database;
using Bot.Database.Dao;

namespace Bot.WebServer.Authentication;

public static class TokenHandler
{
  /// <summary>
  /// Method to generate new unique token.
  /// </summary>
  /// <returns>Unique token</returns>
  public static string GenerateToken()
  {
    var tokenString = "";
    var result = true;
    var dao = new AuthenticationDAO(DbConnection.GetMySqlConnection());
    while (result)
    {
      var token = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(token);
      tokenString = Convert.ToBase64String(token);
      result = dao.CheckToken(tokenString);
    }

    dao.InsertToken(HashToken(tokenString));
    return tokenString;
  }

  /// <summary>
  /// Method to check if a token is valid.
  /// </summary>
  /// <param name="token">Token to check.</param>
  /// <returns>true if token is valid, otherwise false.</returns>
  public static bool CheckToken(string token)
  {
    var dao = new AuthenticationDAO(DbConnection.GetMySqlConnection());
    var result = dao.CheckToken(HashToken(token));
    return result;
  }
  
  private static string HashToken(string token)
  {
    var tokenBytes = Encoding.UTF8.GetBytes(token);
    var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(tokenBytes);
    var hashedToken = Convert.ToBase64String(hashBytes);

    return hashedToken;
  }
}