using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        Log.Information("Web application logger started on {level} level", levelSwitch.MinimumLevel);

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

        //Get tutor information
        app.MapGet("/api/tutors/{tutor?}/{exam?}", FetchTutors).RequireAuthorization();

        // Update a tutors lock
        app.MapPut("/api/tutors/{tutor}/{exam}/{action}", ChangeTutorState).RequireAuthorization();

        // delete a tutor
        app.MapDelete("/api/tutors/{tutor}", DeleteTutor).RequireAuthorization();

        // Map Post requests end points
        app.MapPost("/api/SavePersonCode", SavePersonCode).RequireAuthorization();
        app.MapPost("/api/tutors/upload", async (HttpRequest request, HttpResponse response) =>
        {
            var tutors = new List<Tutoring>();
            try
            {
                tutors = await CsvParser.ParseTutors(request.Body);
            }
            catch (CsvHelper.MissingFieldException e)
            {
                Log.Warning(e.Message);
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            }

            var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());

            foreach (var tutor in tutors)
            {
                if (tutorService.CheckTutorInsertValidity(tutor))
                    continue;
                Log.Warning("Line {i} has invalid values", tutors.IndexOf(tutor));
                response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            tutors.ForEach(tutor => tutorService.InsertNewTutor(tutor));
        }).RequireAuthorization();


        var url = "https://localhost:" + GlobalConfig.WebConfig!.Port;
        app.Run(url);
    }

    private static async void SavePersonCode(HttpRequest request, HttpResponse response)
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
                Log.Debug("Api Request with invalid userId: {id}", pair.UserId);
                response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (!Regex.IsMatch(pair.PersonCode, "^[1-9][0-9]{7}$"))
            {
                Log.Debug("Api Request with invalid PersonCode: {code}", pair.PersonCode);
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
    }

    private static void FetchTutors(string? tutor, string? exam, HttpResponse response)
    {
        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        if (string.IsNullOrEmpty(tutor))
        {
            var tutors = tutorService.FindTutors();
            response.WriteAsync(JsonConvert.SerializeObject(tutors));
            return;
        }

        var tutorObj = tutorService.FindTutoring(tutor);

        if (tutorObj.Count == 0)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (string.IsNullOrEmpty(exam))
        {
            response.WriteAsync(JsonConvert.SerializeObject(tutorObj));
            return;
        }

        var examService = new ExamDAO(DbConnection.GetMySqlConnection());
        if (!examService.FindExam(exam))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        response.WriteAsync(tutorService.IsTutorLocked(tutor, exam, GlobalConfig.BotConfig!.TutorLockHours)
            ? "locked"
            : "unlocked");
    }

    private static async void DeleteTutor(string tutor, HttpResponse response)
    {
        if (string.IsNullOrEmpty(tutor))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        tutorService.DeleteTutor(tutor);
    }

    private static async void ChangeTutorState(string tutor, string exam, string action, HttpResponse response)
    {
        if (string.IsNullOrEmpty(exam) || string.IsNullOrEmpty(tutor) || string.IsNullOrEmpty(action))
        {
            Log.Warning("Received request with an empty parameter");
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        switch (action)
        {
            case "lock":
                tutorService.LockTutor(tutor, exam);
                break;
            case "unlock":
                tutorService.UnlockTutor(tutor, exam);
                break;
            default:
                Log.Warning("Received request with a invalid action");
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;
        }
    }
}