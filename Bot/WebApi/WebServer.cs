using System.Text.RegularExpressions;
using Bot.configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;

namespace Bot.WebApi;

public record struct UserToPersonCode(string UserId, string PersonCode);
public static class WebServer
{
    public static void Init()
    {
        var levelSwitch = new LoggingLevelSwitch();
        var serverLog = new LoggerConfiguration().WriteTo.Console().MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.File("logs/weblog.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        levelSwitch.MinimumLevel = GlobalConfig.GetWebLogLevel();
        Log.Information("Web app logger started on {level} level", levelSwitch.MinimumLevel);
        
        Log.Information("Initializing Web Server...");
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseSerilog(serverLog);
        var app = builder.Build();
        app.MapPost("/api/SavePersonCode", async (HttpRequest request, HttpResponse response) =>
        {
            try
            {
                var pair = await request.ReadFromJsonAsync<UserToPersonCode>();
                if (string.IsNullOrEmpty(pair.UserId) || string.IsNullOrEmpty(pair.PersonCode))
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                if (!Regex.IsMatch(pair.UserId, "^[1-9][0-9]{1,20}$"))
                {
                    serverLog.Information("Api Request with invalid userId: {id}", pair.UserId);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
                
                if (!Regex.IsMatch(pair.PersonCode, "^[1-9][0-9]{7}$"))
                {
                    serverLog.Information("Api Request with invalid PersonCode: {code}", pair.PersonCode);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
                
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }
        });
        var url = "http://localhost:" + GlobalConfig.WebConfig!.Port; 
        app.Run(url);
    }
    
    
}