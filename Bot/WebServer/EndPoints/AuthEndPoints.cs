using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bot.WebServer.EndPoints;

public static class AuthEndPoints
{
  [AllowAnonymous]
  public static async void AuthCallback(HttpContext context, string code, int state)
  {
  
    try
    {
      var oAuthResponse = AuthUtils.GetResponse(code, state, GrantTypeEnum.authorization_code);

      if (oAuthResponse == null)
      {
        Log.Error("OAuth response is null");
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
      }

      //var responseJson = JToken.Parse(oAuthResponse.Content.ReadAsStringAsync().Result);

      if (!oAuthResponse.IsSuccessStatusCode)
      {
        Log.Error("Unsuccessful response from OAuth server: {response}", oAuthResponse);
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        return;
      }

      var token = AuthUtils.GenerateToken();
      
      context.Response.ContentType = "application/json; charset=utf-8";
      await context.Response.WriteAsJsonAsync(new { token = token });
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }
  }
}
