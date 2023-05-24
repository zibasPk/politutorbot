using Bot.configs;
using Bot.WebServer.Authentication;
using Bot.WebServer.EndPoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(TokenAuthOptions.DefaultSchemeName)
      .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
        ("BasicAuthentication", null)
      .AddScheme<TokenAuthOptions,TokenAuthHandler>
        (TokenAuthOptions.DefaultSchemeName, null);

    // Swagger config
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UsePathBase("/tutorapp");
    app.UseRouting();
    app.UseCors("corsapp");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthorization();

    app.MapGet("/", [AllowAnonymous] async (HttpResponse response) =>
    {
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync("OK");
    });
    
    // Authentication Endpoints
    app.MapGet("/auth/callback", AuthEndPoints.AuthCallback);
    app.MapPost("/login", AuthEndPoints.BasicAuthLogin).AllowAnonymous();
    
    
    app.MapGet("/tutor", TutorEndPoints.FetchTutors);
    app.MapGet("/tutoring/{tutorType?}/{exam:int?}", TutoringEndpoints.FetchTutorings);
    app.MapGet("/reservations/{value?}", ReservationEndpoints.FetchReservations);
    app.MapGet("/students", StudentEndpoints.FetchStudents);
    app.MapGet("/course", CourseEndpoints.FetchCourses);
    app.MapGet("/history/{content}/{contentType?}", HistoryEndpoints.FetchHistory);


    // Put endpoint
    app.MapPut("/tutoring/end/{id:int?}/{duration:int?}", TutoringEndpoints.EndTutoringAction);
    app.MapPut("/tutor/{tutorCode:int}/contract/{state:int}", TutorEndPoints.HandleContractAction);
    app.MapPut("/reservations/{id:int}/{action}", ReservationEndpoints.HandleReservationAction);

    // Post endpoints
    app.MapPost("/tutoring/start/{tutorCode:int?}/{studentCode:int?}/{examCode:int?}", TutoringEndpoints.StartTutoringAction);
    app.MapPost("/tutor/{action}", TutorEndPoints.HandleTutorAction);
    app.MapPost("/exam/{action}", ExamEndpoints.HandleExamAction);
    app.MapPost("/course/{action}", CourseEndpoints.HandleCourseAction);
    app.MapPost("/students/{action}/{studentCode:int?}", StudentEndpoints.HandleStudentAction);
    app.MapPut("/reservations/{action}", ReservationEndpoints.HandleReservationAction);
    
    // Delete endpoints
    app.MapDelete("/tutoring/", TutoringEndpoints.RemoveTutoring);
    app.MapDelete("/tutors", TutorEndPoints.DeleteTutors);
    app.MapDelete("/exams", ExamEndpoints.DeleteExams);
    app.MapDelete("/courses", CourseEndpoints.DeleteCourses);


    var url = GlobalConfig.WebConfig!.Url + ":" + GlobalConfig.WebConfig.Port;
    app.Urls.Add(url);
    app.Run();
  }
}