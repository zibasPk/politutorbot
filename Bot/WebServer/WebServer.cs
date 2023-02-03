using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Bot.WebServer.EndPoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Bot.WebServer;

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

    // Add server logger
    builder.Host.UseSerilog(serverLog);

    // CORS handler
    builder.Services.AddCors(p =>
      p.AddPolicy("corsapp",
        policyBuilder => { policyBuilder.WithOrigins(GlobalConfig.WebConfig!.AllowedCors).AllowAnyMethod().AllowAnyHeader(); }));

    // Authorization handler
    builder.Services.AddAuthentication("BasicAuthentication")
      .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
        ("BasicAuthentication", null);
    builder.Services.AddAuthorization();

    // Swagger config
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UsePathBase("/tutorapp");
    app.UseRouting();
    app.UseCors("corsapp");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();

    // Get endpoints
    app.MapGet("/", async (HttpResponse response) =>
    {
      response.ContentType = "application/json; charset=utf-8";
      await response.WriteAsync("OK");
    });
    app.MapGet("/tutor", TutorEndPoints.FetchTutors).RequireAuthorization();
    app.MapGet("/tutoring/{tutorType?}/{exam:int?}", TutoringEndpoints.FetchTutorings).RequireAuthorization();
    app.MapGet("/reservations/{value?}", ReservationEndpoints.FetchReservations).RequireAuthorization();
    app.MapGet("/students", StudentEndpoints.FetchStudents).RequireAuthorization();
    app.MapGet("/course", CourseEndpoints.FetchCourses).RequireAuthorization();
    app.MapGet("/history/{content}/{contentType?}", FetchHistory).RequireAuthorization();


    // Put endpoint
    app.MapPut("/tutoring/end/{id:int?}/{duration:int?}", TutoringEndpoints.EndTutoringAction)
      .RequireAuthorization();
    app.MapPut("/tutor/{tutorCode:int}/contract/{state:int}", TutorEndPoints.HandleContractAction).RequireAuthorization();
    app.MapPut("/reservations/{id:int}/{action}", ReservationEndpoints.HandleReservationAction).RequireAuthorization();

    // Post endpoints
    app.MapPost("/tutoring/start/{tutorCode:int?}/{studentCode:int?}/{examCode:int?}", TutoringEndpoints.StartTutoringAction)
      .RequireAuthorization();
    app.MapPost("/tutor/{action}", TutorEndPoints.HandleTutorAction).RequireAuthorization();
    app.MapPost("/exam/{action}", ExamEndpoints.HandleExamAction).RequireAuthorization();
    app.MapPost("/course/{action}", CourseEndpoints.HandleCourseAction).RequireAuthorization();
    app.MapPost("/students/{action}/{studentCode:int?}", StudentEndpoints.HandleStudentAction).RequireAuthorization();

    // Delete endpoints
    app.MapDelete("/tutoring/", TutoringEndpoints.RemoveTutoring).RequireAuthorization();
    app.MapDelete("/tutors", TutorEndPoints.DeleteTutors).RequireAuthorization();
    app.MapDelete("/exams", ExamEndpoints.DeleteExams).RequireAuthorization();
    app.MapDelete("/courses", CourseEndpoints.DeleteCourses).RequireAuthorization();


    var url = GlobalConfig.WebConfig!.Url + ":" + GlobalConfig.WebConfig.Port;
    app.Urls.Add(url);
    app.Run();
  }

  private static void FetchHistory(string content, string? contentType, HttpResponse response)
  {
    try
    {
      var historyService = new HistoryDAO(DbConnection.GetMySqlConnection());
      switch (content)
      {
        case "tutorings":
          if (contentType == "active")
          {
            var activeTutoringHistory = historyService.FindActiveTutoringHistory(out var activeHeaders);
            var result = new { header = activeHeaders, content = activeTutoringHistory };
            response.ContentType = "application/json; charset=utf-8";
            response.WriteAsync(JsonConvert.SerializeObject(result));
            return;
          }

          var tutoringHistory = historyService.FindTutoringHistory(out var tutoringHeaders);
          var tutoringResult = new { header = tutoringHeaders, content = tutoringHistory };
          response.ContentType = "application/json; charset=utf-8";
          response.WriteAsync(JsonConvert.SerializeObject(tutoringResult));
          return;
        case "reservations":
          var reservationHistory = historyService.FindReservationHistory(out var reservationHeaders);
          var reservationResult = new { header = reservationHeaders, content = reservationHistory };
          response.ContentType = "application/json; charset=utf-8";
          response.WriteAsync(JsonConvert.SerializeObject(reservationResult));
          return;
        default:
          response.StatusCode = StatusCodes.Status404NotFound;
          return;
      }
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
}