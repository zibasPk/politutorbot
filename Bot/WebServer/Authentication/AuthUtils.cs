using System.Security.Cryptography;
using System.Text;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;

namespace Bot.WebServer.Authentication;

public enum GrantTypeEnum
{
  authorization_code,
  refresh_token
}

public static class AuthUtils
{
  public static HttpResponseMessage? GetResponse(string code, int state, GrantTypeEnum grantType)
  {
    HttpClient httpClient = new();

    var clientSecret = GlobalConfig.WebConfig!.AzureSecret;
    if (clientSecret == "") return null;

    var formContent = new Dictionary<string, string>
    {
      { "client_id", GlobalConfig.WebConfig!.AzureClientId },
      { "client_secret", clientSecret },
      { grantType == GrantTypeEnum.authorization_code ? "code" : "refresh_token", code },
      { "grant_type", grantType.ToString() }
    };

    //non rimuovere - test
    if (grantType == GrantTypeEnum.authorization_code)
      switch (state)
      {
        case 10020:
        {
          formContent.Add("redirect_uri",
            "https://zibasPk.github.io/PoliTutorApp/");
          break;
        }

        default:
        {
          formContent.Add("redirect_uri", "https://api.polinetwork.org" + "/tutorapp/" + "auth/callback");
          break;
        }
      }

    var formUrlEncodedContent = new FormUrlEncodedContent(formContent);
    return httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token",
      formUrlEncodedContent).Result;
  }
  
  /// <summary>
  /// Method to generate new unique token. And saves it in db.
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