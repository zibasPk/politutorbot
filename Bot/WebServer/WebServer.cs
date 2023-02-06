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

    // Authentication Endpoints
    app.MapGet("/auth/callback", AuthEndPoints.AuthCallback);

    // Login endpoint
    app.MapPost("/login", async (HttpResponse response, [FromServices] IAuthenticationService authenticationService) =>
    {
      var result = await authenticationService.AuthenticateAsync(response.HttpContext, "BasicAuthentication");
      if (result.Succeeded)
      {
        try
        {
          var token = AuthUtils.GenerateToken();
          response.ContentType = "application/json; charset=utf-8";
          await response.WriteAsync(JsonConvert.SerializeObject(new { token = token }));
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
    }).AllowAnonymous();
    
    app.MapGet("/", [AllowAnonymous] async (HttpResponse response) =>
    {
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync("OK");
    });
    
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