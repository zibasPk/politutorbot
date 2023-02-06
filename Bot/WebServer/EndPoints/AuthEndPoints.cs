using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bot.WebServer.EndPoints;

public static class AuthEndPoints
{
  public static async void AuthCallback(HttpContext context)
  {
    var code = context.Request.Query["code"];
    var state = context.Request.Query["state"];
    try
    {
      var oAuthResponse = AuthUtils.GetResponse(code, int.Parse(state), GrantTypeEnum.authorization_code);

      if (oAuthResponse == null)
      {
        Log.Error("OAuth response is null");
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
      }

      var responseJson = JToken.Parse(oAuthResponse.Content.ReadAsStringAsync().Result);

      if (!oAuthResponse.IsSuccessStatusCode)
      {
        Log.Error("Unsuccessful response from OAuth server: {response}", responseJson);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return;
      }

      var token = AuthUtils.GenerateToken();

      var responseObject = new JObject
      {
        { "access_token", token }
      };
      await context.Response.WriteAsJsonAsync(responseObject);
      return;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      return;
    }
  }
}
