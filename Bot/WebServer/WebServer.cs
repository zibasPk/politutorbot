using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Bot.Database.Records;
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
    // CORS handler
    if (GlobalConfig.WebConfig!.AllowCors)
    {
      builder.Services.AddCors(p =>
        p.AddPolicy("corsapp", policyBuilder => { policyBuilder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader(); }));
    }

    // Authorization handler
    builder.Services.AddAuthentication("BasicAuthentication")
      .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
        ("BasicAuthentication", null);
    builder.Services.AddAuthorization();

    var app = builder.Build();

    if (GlobalConfig.WebConfig!.AllowCors)
    {
      app.UseCors("corsapp");
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpsRedirection();

    //Get tutor information
    app.MapGet("/api/tutor", FetchTutors).RequireAuthorization();
    app.MapGet("/api/tutoring/{tutorType?}/{exam:int?}", FetchTutorings).RequireAuthorization();
    app.MapGet("/api/reservations/{value?}", FetchReservations).RequireAuthorization();
    app.MapGet("/api/students", FetchStudents).RequireAuthorization();
    app.MapGet("/api/course", FetchCourses).RequireAuthorization();


    // Update a tutors lock
    app.MapPut("/api/tutoring/{action}/{id:int?}/{duration:int?}", TutoringAction)
      .RequireAuthorization();
    app.MapPut("/api/reservations/{id:int}/{action}", HandleReservationAction).RequireAuthorization();
    //app.MapPut("/api/tutors/{tutor}/{exam}/{action}", ChangeTutorState).RequireAuthorization();

    // delete a tutor
    //app.MapDelete("/api/tutors/{tutor}", DeleteTutor).RequireAuthorization();

    // Map Post requests end points
    app.MapPost("/api/students/{action}/{studentCode:int?}", HandleStudentAction).RequireAuthorization();
    // app.MapPost("/api/SavePersonCode", SavePersonCode).RequireAuthorization();

    var url = "https://localhost:" + GlobalConfig.WebConfig!.Port;
    app.Run(url);
  }


  private static void HandleReservationAction(int id, string action, HttpResponse response)
  {
    if (action is not ("confirm" or "refuse"))
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var reservationService = new ReservationDAO(DbConnection.GetMySqlConnection());

    if (reservationService.FindReservation(id) == null)
    {
      // There is no Reservation with the given id
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"no reservation with id: {id} found");
      return;
    }

    if (reservationService.IsReservationProcessed(id))
    {
      // Reservation already marked as processed
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"reservation {id} already processed");
      return;
    }

    if (action == "refuse")
    {
      reservationService.RefuseReservation(id);
      return;
    }

    if (!reservationService.IsReservationAllowed(id))
    {
      // The exam corresponding to the reservation has no available reservations
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"tutor for reservation {id} hasn't enough available reservations for this exam.");
      return;
    }

    tutorService.ActivateTutoring(id);
  }


  private static async void TutoringAction(string action, int? id, int? duration, HttpResponse response,
    HttpRequest request)
  {
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    if (action != "end")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    if (!id.HasValue)
    {
      // End all given tutorings
      var durations = await request.ReadFromJsonAsync<List<TutoringToDuration>>();

      if (durations == null)
      {
        // invalid request body
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"invalid request body");
        return;
      }

      var exceedingDuration = durations.Find(x => x.Duration > GlobalConfig.BotConfig!.MaxTutoringDuration);
      if (exceedingDuration != default)
      {
        // invalid duration in request body
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync(
          $"invalid duration for tutoring: {exceedingDuration.Id} in request body the maximum " +
          $"is: {GlobalConfig.BotConfig!.MaxTutoringDuration} hours");
        return;
      }

      tutorService.EndTutorings(durations);
      return;
    }

    if (duration == null || duration > GlobalConfig.BotConfig!.MaxTutoringDuration)
    {
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid duration parameter");
      return;
    }

    if (tutorService.EndTutoring(id.Value, duration.Value))
      return;

    response.StatusCode = StatusCodes.Status400BadRequest;
    await response.WriteAsync($"no tutoring with id {id.Value} found");
  }

  private static async void HandleStudentAction(string action, int? studentCode, HttpResponse response,
    HttpRequest request)
  {
    if (action != "enable" && action != "disable")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    var studentService = new StudentDAO(DbConnection.GetMySqlConnection());

    if (!studentCode.HasValue)
    {
      // Enable all given students
      List<int>? studentCodes = null;
      try
      {
        studentCodes = await request.ReadFromJsonAsync<List<int>>();
      }
      catch (Exception)
      {
        // Invalid request body
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"invalid request body");
        return;
      }

      if (studentCodes == null)
      {
        // Invalid request body
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"invalid request body");
        return;
      }

      var badCode = studentCodes.Find(x => !Regex.IsMatch(x.ToString(), "^[1-9][0-9]{5}$"));

      if (badCode != default)
      {
        // Student list contains an invalid student code format
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"invalid student code: {badCode} in request body");
        return;
      }
      
      switch (action)
      {
        case "enable":
          if (studentService.EnableStudent(studentCodes)) return;
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"duplicate student code in request body");
          return;
        case "disable":
          if (studentService.DisableStudent(studentCodes)) return;
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"tried disabling non enabled student code");
          return;
        default:
          response.StatusCode = StatusCodes.Status404NotFound;
          return;
      }
    }
    
    if (!Regex.IsMatch(studentCode.Value.ToString(), "^[1-9][0-9]{5}$"))
    {
      // Request contains an invalid student code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid student code: {studentCode.Value} in request");
      return;
    }
    
    switch (action)
    {
      case "enable":
        if (studentService.EnableStudent(studentCode.Value))
          return;
        
        // Request contains a duplicate student code
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"tried enabling already enabled student code: {studentCode.Value}");
        return;
      case "disable":
        if (studentService.DisableStudent(studentCode.Value))
          return;
        
        // Request contains a duplicate student code
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"tried disabling non enabled student code: {studentCode.Value}");
        return;
      default:
        response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
  }

  private static void FetchTutors(HttpResponse response)
  {
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    response.WriteAsync(JsonConvert.SerializeObject(tutorService.FindTutors()));
  }
  private static void FetchReservations(string? value, HttpResponse response)
  {
    var reservationService = new ReservationDAO(DbConnection.GetMySqlConnection());

    var returnObject = "";
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
          if (reservation != null)
            returnObject = JsonConvert.SerializeObject(reservation);
        }

        break;
    }

    response.WriteAsync(returnObject);
  }

  private static void FetchStudents(HttpResponse response)
  {
    var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
    response.WriteAsync(JsonConvert.SerializeObject(studentService.FindEnabledStudents()));
  }
  
  private static void FetchCourses(HttpResponse response)
  {
    var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
    response.WriteAsync(JsonConvert.SerializeObject(courseService.FindCourses()));
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

  private static void FetchTutorings(string? tutorType, int? exam, HttpResponse response)
  {
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    if (tutorType == null)
    {
      // Return all possible tutorings
      var tutors = tutorService.FindTutorings();
      response.WriteAsync(JsonConvert.SerializeObject(tutors));
      return;
    }

    List<ActiveTutoring> activeTutoringsList;
    switch (tutorType)
    {
      case "active":
        // Return all active tutorings
        activeTutoringsList = tutorService.FindActiveTutorings(true);
        response.WriteAsync(JsonConvert.SerializeObject(activeTutoringsList));
        return;
      case "ended":
        // Return all ended tutorings
        activeTutoringsList = tutorService.FindActiveTutorings(false);
        response.WriteAsync(JsonConvert.SerializeObject(activeTutoringsList));
        return;
    }

    if (!Regex.IsMatch(tutorType, "^[1-9][0-9]{7}$"))
    {
      // Invalid tutorType parameter
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"invalid param: {tutorType}");
      return;
    }

    var tutorCode = int.Parse(tutorType);
    var tutoringList = tutorService.FindTutorings(tutorCode);

    if (tutoringList.Count == 0)
    {
      // Given tutor has no tutorings
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"no tutorings for tutor with code: {tutorCode} found");
      return;
    }

    if (!exam.HasValue)
    {
      // Return all possible tutorings from a tutor
      response.WriteAsync(JsonConvert.SerializeObject(tutoringList));
      return;
    }

    var examService = new ExamDAO(DbConnection.GetMySqlConnection());
    if (!examService.FindExam(exam.Value))
    {
      // Exam doesn't exist
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"no exam with code: {exam.Value} found");
      return;
    }

    var tutoring = tutorService.FindTutoring(tutorCode, exam.Value);

    if (tutoring == null)
    {
      // Tutoring doesn't exist
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"no tutoring from tutor: {tutorCode} for exam: {exam.Value} found");
      return;
    }

    // Return tutoring from tutor for exam
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