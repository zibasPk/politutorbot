using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
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
    builder.Services.AddCors(p =>
        p.AddPolicy("corsapp", policyBuilder => { policyBuilder.WithOrigins(GlobalConfig.WebConfig!.AllowedCors).AllowAnyMethod().AllowAnyHeader(); }));

      // Authorization handler
    builder.Services.AddAuthentication("BasicAuthentication")
      .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
        ("BasicAuthentication", null);
    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseCors("corsapp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpsRedirection();

    // Get endpoints
    app.MapGet("/api/tutor", FetchTutors).RequireAuthorization();
    app.MapGet("/api/tutoring/{tutorType?}/{exam:int?}", FetchTutorings).RequireAuthorization();
    app.MapGet("/api/reservations/{value?}", FetchReservations).RequireAuthorization();
    app.MapGet("/api/students", FetchStudents).RequireAuthorization();
    app.MapGet("/api/course", FetchCourses).RequireAuthorization();


    // Put endpoint
    app.MapPut("/api/tutoring/end/{id:int?}/{duration:int?}", EndTutoringAction)
      .RequireAuthorization();
    app.MapPut("/api/tutor/{tutorCode:int}/contract/{state:int}", HandleContractAction).RequireAuthorization();
    app.MapPut("/api/reservations/{id:int}/{action}", HandleReservationAction).RequireAuthorization();

    // Post endpoints
    app.MapPost("/api/tutoring/start/{tutorCode:int?}/{studentCode:int?}/{examCode:int?}", StartTutoringAction).RequireAuthorization();
    app.MapPost("/api/tutor/{action}", HandleTutorAction).RequireAuthorization();
    app.MapPost("/api/students/{action}/{studentCode:int?}", HandleStudentAction).RequireAuthorization();
    
    // app.MapPost("/api/SavePersonCode", SavePersonCode).RequireAuthorization();
    app.MapDelete("/api/tutoring/", RemoveTutoring).RequireAuthorization();
    app.MapDelete("/api/tutors", DeleteTutors).RequireAuthorization();


    app.UsePathBase("/tutorapp");
    var url = GlobalConfig.WebConfig!.Url +  ":" + GlobalConfig.WebConfig.Port;
    app.Urls.Add(url);
    app.Run();
  }

  private static async void DeleteTutors(HttpResponse response, HttpRequest request)
  {
    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      
      tutorService.DeleteTutors();
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
  

  private static async void RemoveTutoring(HttpResponse response, HttpRequest request)
  {
    // Enable all given students
    List<TutorCodeToExamCode>? tutorings = null;
    try
    {
      tutorings = await request.ReadFromJsonAsync<List<TutorCodeToExamCode>>();
    }
    catch (Exception e)
    {
      // Invalid request body
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid request body");
      return;
    }

    if (tutorings == null)
    {
      // Invalid request body
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid request body");
      return;
    }
    
    var badCode = tutorings.Find(x => !Regex.IsMatch(x.TutorCode.ToString(), RegularExpr.StudentCode));
    
    if (badCode != default)
    {
      // Student list contains an invalid student code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid tutor code: {badCode.TutorCode} in request body");
      return;
    }

    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      
      if (tutorService.DeleteTutorings(tutorings, out var errorMessage)) 
        return;
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"Invalid tutorings in request body: {errorMessage}");
      return;
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }


  private static void HandleContractAction(int tutorCode, int state, HttpResponse response)
  {
    if (state is > 2 or < 0)
    {
      // Not a valid contract state
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.WriteAsync($"Invalid state for contract with tutor: {tutorCode}");
      return;
    }

    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      if (tutorService.ChangeContractState(tutorCode, state))
        return; 
      response.StatusCode = StatusCodes.Status502BadGateway;
      response.WriteAsync($"Error in contract state change for tutor: {tutorCode}");
      return;
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
      return;
    }
  }

  private static void HandleReservationAction(int id, string action, HttpResponse response)
  {
    if (action is not ("confirm" or "refuse"))
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    try
    {
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

      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      tutorService.ActivateTutoring(id);
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }


  private static async void EndTutoringAction(int? id, int? duration, HttpResponse response,
    HttpRequest request)
  {
    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());

      if (!id.HasValue)
      {
        // End all given tutorings
        List<TutoringToDuration>? durations;
        try
        {
          durations = await request.ReadFromJsonAsync<List<TutoringToDuration>>();
        }
        catch (Exception)
        {
          // Invalid request body
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"invalid request body");
          return;
        }

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
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static async void StartTutoringAction(int? tutorCode, int? studentCode, int? examCode, HttpResponse response,
    HttpRequest request)
  {
    try
    {
      // Check if not for a single request
      if (!tutorCode.HasValue)
      {
        // End all given tutorings
        List<TutorToStudentToExam>? tutorings;
        try
        {
          // Try parsing body
          tutorings = await request.ReadFromJsonAsync<List<TutorToStudentToExam>>();
        }
        catch (Exception e)
        {
          // Invalid request body
          Console.Write(e);
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"invalid request body");
          return;
        }

        foreach (var data in tutorings!)
        {
          if (!await StartTutoring(data, response))
          {
            return;
          }
        }

        return;
      }

      if (!studentCode.HasValue)
      {
        // Invalid request body
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"student code missing in url");
        return;
      }

      await StartTutoring(
        new TutorToStudentToExam(examCode.HasValue, tutorCode!.Value, studentCode.Value, examCode),
        response);
      return;
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static async Task<bool> StartTutoring(TutorToStudentToExam tutoringData, HttpResponse response)
  {
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
    // Check if student is enabled
    if (!studentService.IsStudentEnabled(tutoringData.StudentCode))
    {
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"Student: {tutoringData.StudentCode} isn't enabled for tutoring");
      return false;
    }

    if (tutoringData.IsOFA)
    {
      // OFA tutoring
      var tutor = tutorService.FindTutor(tutoringData.TutorCode);

      // Check if tutor exists and is available for OFA
      if (tutor is not { OfaAvailable: true })
      {
        // Tutor isn't available for OFA or doesn't exist
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"Tutor: {tutoringData.TutorCode} isn't available for OFA");
        return false;
      }

      // Start tutoring
      try
      {
        tutorService.ActivateTutoring(tutoringData.TutorCode, tutoringData.StudentCode);
      }
      catch (MySqlException e)
      {
        // This should be caused by a trigger on the active_tutoring table
        if (e.Number != 45000)
          throw;
        // Tutor isn't available for OFA or doesn't exist
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"Tutoring from Tutor: {tutoringData.TutorCode} for Student: {tutoringData.StudentCode} is already active");
        return false;
      }

      return true;
    }

    // Normal tutoring
    var tutoring = tutorService.FindTutoring(tutoringData.TutorCode, tutoringData.ExamCode!.Value);

    if (!tutoring.HasValue)
    {
      // tutoring doesn't exist
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"Tutor: {tutoringData.TutorCode} doesn't exist");
      return false;
    }

    if (tutoring.Value.AvailableTutorings < 1)
    {
      // tutoring doesn't have enough available tutorings
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"Tutor: {tutoringData.TutorCode} doesn't have enough available tutorings");
      return false;
    }

    // Start tutoring
    tutorService.ActivateTutoring(tutoringData.TutorCode, tutoringData.StudentCode, tutoringData.ExamCode.Value);
    return true;
  }

  private static async void HandleStudentAction(string action, int? studentCode, HttpResponse response,
    HttpRequest request)
  {
    if (action != "enable" && action != "disable")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    try
    {
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

        var badCode = studentCodes.Find(x => !Regex.IsMatch(x.ToString(), RegularExpr.StudentCode));

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

      if (!Regex.IsMatch(studentCode.Value.ToString(), RegularExpr.StudentCode))
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
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static async void HandleTutorAction(string action, HttpResponse response, HttpRequest request)
  {
    if (action != "add" && action != "remove")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    // Read request body
    List<TutorToExam>? tutorings = null;
    try
    {
      tutorings = await request.ReadFromJsonAsync<List<TutorToExam>>();
    }
    catch (Exception e)
    {
      // Invalid request body
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid request body");
      return;
    }

    if (tutorings == null)
    {
      // Invalid request body
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid request body");
      return;
    }

    var badCode = tutorings.Find(x => !Regex.IsMatch(x.TutorCode.ToString(), RegularExpr.StudentCode));

    if (badCode != default)
    {
      // Student list contains an invalid student code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid tutor code: {badCode.TutorCode} in request body");
      return;
    }

    var badRanking = tutorings.Find(x => x.Ranking < 1);

    if (badRanking != default)
    {
      // Student list contains an invalid student code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid ranking: {badRanking} in request body");
      return;
    }

    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());

      switch (action)
      {
        case "add":
          if (tutorService.AddTutor(tutorings, out var errorMessage)) return;
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"invalid tutorings in request body: {errorMessage}");
          return;
        case "remove":
          // if (tutorService.AddTutor(tutorings[0])) return;
          // response.StatusCode = StatusCodes.Status400BadRequest;
          // await response.WriteAsync($"tried disabling non enabled student code");
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

  private static void FetchTutors(HttpResponse response)
  {
    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      response.WriteAsync(JsonConvert.SerializeObject(tutorService.FindTutors()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static void FetchReservations(string? value, HttpResponse response)
  {
    try
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
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static void FetchStudents(HttpResponse response)
  {
    try
    {
      var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
      response.WriteAsync(JsonConvert.SerializeObject(studentService.FindEnabledStudents()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  private static void FetchCourses(HttpResponse response)
  {
    try
    {
      var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
      response.WriteAsync(JsonConvert.SerializeObject(courseService.FindCourses()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
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

      if (!Regex.IsMatch(pair.UserId, RegularExpr.TelegramUserId))
      {
        Log.Debug("Api Request with invalid userId: {id}", pair.UserId);
        response.StatusCode = StatusCodes.Status400BadRequest;
        return;
      }

      if (!Regex.IsMatch(pair.PersonCode, RegularExpr.PersonCode))
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
    try
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

      if (!Regex.IsMatch(tutorType, RegularExpr.StudentCode))
      {
        // Tutor type isn't a studentCode
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
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
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