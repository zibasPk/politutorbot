using Bot.configs;
using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bot.WebServer.EndPoints;

public static class AuthEndPoints
{
  [AllowAnonymous]
  public static async void BasicAuthLogin(HttpResponse response, [FromServices] IAuthenticationService authenticationService)
  {
    var result = await authenticationService.AuthenticateAsync(response.HttpContext, "BasicAuthentication");
    if (result.Succeeded)
    {
      try
      {
        var token = AuthUtils.GenerateToken();
        response.ContentType = "application/json; charset=utf-8";
        await response.WriteAsJsonAsync(new { token = token , expiresIn = GlobalConfig.WebConfig!.TokenValidityDays});
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        response.StatusCode = e switch
        {
          MySqlException _ => StatusCodes.Status502BadGateway,
          _ => StatusCodes.Status500InternalServerError
        };
      }
    }
    else
    {
      response.StatusCode = 401;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync("Unauthorized");
    }
  }
  
  
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

      // Print the response
      var responseString = await oAuthResponse.Content.ReadAsStringAsync();
      Log.Information("OAuth response: {response}", responseString);
      if (!oAuthResponse.IsSuccessStatusCode)
      {
        Log.Error("Unsuccessful response from OAuth server: {response}", oAuthResponse);
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        return;
      }

      var token = AuthUtils.GenerateToken();
      
      context.Response.ContentType = "application/json; charset=utf-8";
      await context.Response.WriteAsJsonAsync(new { token = token , expiresIn = GlobalConfig.WebConfig!.TokenValidityDays});
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }
  }
}
