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
        app.MapGet("/api/tutoring/{tutor:int?}/{exam:int?}", FetchTutors).RequireAuthorization();
        app.MapGet("/api/reservations/{value?}", FetchReservations).RequireAuthorization();

        // Update a tutors lock
        //app.MapPut("/api/tutoring/{tutor:int}/{exam:int}/{student}/{action}", TutoringAction).RequireAuthorization();
        app.MapPut("/api/reservations/{id:int}/{action}", HandleReservationAction).RequireAuthorization();
        //app.MapPut("/api/tutors/{tutor}/{exam}/{action}", ChangeTutorState).RequireAuthorization();

        // delete a tutor
        //app.MapDelete("/api/tutors/{tutor}", DeleteTutor).RequireAuthorization();

        // Map Post requests end points
        // app.MapPost("/api/SavePersonCode", SavePersonCode).RequireAuthorization();

        var url = "https://localhost:" + GlobalConfig.WebConfig!.Port;
        app.Run(url);
    }

   

    private static void HandleReservationAction(int id, string action, HttpResponse response)
    {
        if (action != "confirm")
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        var reservationService = new ReservationDAO(DbConnection.GetMySqlConnection());
        // Checks if the exam corresponding to the reservation has any available reservations
        if (!reservationService.IsReservationAllowed(id))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.WriteAsync($"reservation {id} hasn't enough available reservations.");
            return;
        }

        tutorService.ActivateTutoring(id);

        return;
    }

    private static void FetchReservations(string? value, HttpResponse response)
    {
        var reservationService = new ReservationDAO(DbConnection.GetMySqlConnection());

        string returnObject = "";
        switch (value)
        {
            case null:
                returnObject = JsonConvert.SerializeObject(reservationService.FindReservations());
                break;
            case "not-processed":
                returnObject = JsonConvert.SerializeObject(reservationService.FindReservations(false));
                break;
            case "processed":
                returnObject = JsonConvert.SerializeObject(reservationService.FindReservations(true));
                break;
            default:
                if (int.TryParse(value, out var reservationId))
                {
                    var reservation = reservationService.FindReservation(reservationId);
                    if (reservation.HasValue)
                        returnObject = JsonConvert.SerializeObject(reservation);
                }

                break;
        }

        response.WriteAsync(returnObject);
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

    private static void FetchTutors(int? tutor, int? exam, HttpResponse response)
    {
        var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
        if (tutor == null)
        {
            var tutors = tutorService.FindTutorings();
            response.WriteAsync(JsonConvert.SerializeObject(tutors));
            return;
        }

        var tutoringList = tutorService.FindTutorings(tutor.Value);

        if (tutoringList.Count == 0)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (exam == null)
        {
            response.WriteAsync(JsonConvert.SerializeObject(tutoringList));
            return;
        }

        var examService = new ExamDAO(DbConnection.GetMySqlConnection());
        if (!examService.FindExam(exam.Value))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }


        var tutoring = tutorService.FindTutoring(tutor.Value, exam.Value);
        response.WriteAsync(JsonConvert.SerializeObject(tutoring));
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