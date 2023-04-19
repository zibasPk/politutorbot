using System.IdentityModel.Tokens.Jwt;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
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
    public static async void BasicAuthLogin(HttpResponse response,
        [FromServices] IAuthenticationService authenticationService)
    {
        var result = await authenticationService.AuthenticateAsync(response.HttpContext, "BasicAuthentication");
        if (result.Succeeded)
        {
            try
            {
                var token = AuthUtils.GenerateToken();
                response.ContentType = "application/json; charset=utf-8";
                await response.WriteAsJsonAsync(new
                    { token = token, expiresIn = GlobalConfig.WebConfig!.TokenValidityDays });
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
            HttpClient httpClient = new();
            var oAuthResponse = AuthUtils.GetAzureResponse(code, state);
            if (oAuthResponse == null)
            {
                Log.Error("OAuth response is null");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (!oAuthResponse.IsSuccessStatusCode)
            {
                Log.Error("Unsuccessful response from OAuth server: {response}", oAuthResponse);
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }

            var responseJson = JObject.Parse(await oAuthResponse.Content.ReadAsStringAsync());
            responseJson.TryGetValue("access_token", out var accessToken);
            var strToken = accessToken.Value<string>();
            // Get the user mail from the access token
            var handler = new JwtSecurityTokenHandler();
            var email = handler.ReadJwtToken(strToken).Payload.Claims.First(e => e.Type == "unique_name").Value;
            var tenantId = handler.ReadJwtToken(strToken).Payload.Claims.First(e => e.Type == "tid").Value;

            // Check if the users' email tenant is authorized to access the application
            if (!GlobalConfig.WebConfig!.AllowedTenants.Contains(tenantId))
            {
                Log.Warning("User with mail: {userMail} tried accessing the application from an unauthorized tenant.",
                    email);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var authDao = new AuthenticationDAO(DbConnection.GetMySqlConnection());
            // Check if the user specific email is authorized to access the application
            if (!authDao.VerifyMail(email))
            {
                Log.Information("User with Unauthorized mail: {email} tried accessing the application.", email);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Generate a token for the user
            var token = AuthUtils.GenerateToken();
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new
                { token = token, expiresIn = GlobalConfig.WebConfig!.TokenValidityDays });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}