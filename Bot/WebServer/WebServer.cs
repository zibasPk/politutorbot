using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace Bot.WebServer;

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

        // Authorization handler
        builder.Services.AddAuthentication("BasicAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
                ("BasicAuthentication", null);
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
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

                //TODO: insert person code into db ( db needs to be changed)
                MessageHandlers.OnApiCall(int.Parse(pair.UserId));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }).RequireAuthorization();

        app.MapPost("/api/tutors", FetchTutors).RequireAuthorization();
        app.MapPost("/api/delTutor", DeleteTutor).RequireAuthorization();
        app.MapPost("/api/unlockTutor", UnlockTutor).RequireAuthorization();

        var url = "http://localhost:" + GlobalConfig.WebConfig!.Port;
        app.Run(url);
    }

    private static List<Tutor> FetchTutors(HttpRequest request, HttpResponse response)
    {
        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        var tutors = tutorService.FindTutors();
        return tutors; 
    }

    private static async void DeleteTutor(HttpRequest request, HttpResponse response)
    {
        string? tutor;
        try
        {
            tutor = await request.ReadFromJsonAsync<string>();
        }
        catch (Exception e)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            Log.Error("Error while parsing body");
            throw;
        }

        if (string.IsNullOrEmpty(tutor))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        tutorService.DeleteTutor(tutor);
        return;
    }
    
    private static async void UnlockTutor(HttpRequest request, HttpResponse response)
    {
        string[]? tutorAndExam;
        try
        {
            tutorAndExam = await request.ReadFromJsonAsync<string[]>();
        }
        catch (Exception e)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            Log.Error("Error while parsing body");
            throw;
        }
            
        if (tutorAndExam == null || string.IsNullOrEmpty(tutorAndExam[0]) || string.IsNullOrEmpty(tutorAndExam[1]))
        {
            Log.Warning("received request with empty qualcosa");
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        tutorService.UnlockTutor(tutorAndExam[0], tutorAndExam[1]);
        return;
    }
}