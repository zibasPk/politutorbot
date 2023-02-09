using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

[Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
public static class TutoringEndpoints
{
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static void FetchTutorings(string? tutorType, int? exam, HttpResponse response)
  {
    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      if (tutorType == null)
      {
        // Return all possible tutorings
        var tutors = tutorService.FindTutorings();
        response.ContentType = "application/json; charset=utf-8";
        response.WriteAsync(JsonConvert.SerializeObject(tutors));
        return;
      }

      List<ActiveTutoring> activeTutoringsList;
      switch (tutorType)
      {
        case "active":
          // Return all active tutorings
          activeTutoringsList = tutorService.FindActiveTutorings(true);
          response.ContentType = "application/json; charset=utf-8";
          response.WriteAsync(JsonConvert.SerializeObject(activeTutoringsList));
          return;
        case "ended":
          // Return all ended tutorings
          activeTutoringsList = tutorService.FindActiveTutorings(false);
          response.ContentType = "application/json; charset=utf-8";
          response.WriteAsync(JsonConvert.SerializeObject(activeTutoringsList));
          return;
      }

      if (!Regex.IsMatch(tutorType, RegularExpr.StudentCode))
      {
        // Tutor type isn't a studentCode
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
        response.WriteAsync($"invalid param: {tutorType}");
        return;
      }

      var tutorCode = int.Parse(tutorType);
      var tutoringList = tutorService.FindTutorings(tutorCode);

      if (tutoringList.Count == 0)
      {
        // Given tutor has no tutorings
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
        response.WriteAsync($"no tutorings for tutor with code: {tutorCode} found");
        return;
      }

      if (!exam.HasValue)
      {
        // Return all possible tutorings from a tutor
        response.ContentType = "application/json; charset=utf-8";
        response.WriteAsync(JsonConvert.SerializeObject(tutoringList));
        return;
      }

      var examService = new ExamDAO(DbConnection.GetMySqlConnection());

      if (!examService.FindExam(exam.Value))
      {
        // Exam doesn't exist
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
        response.WriteAsync($"no exam with code: {exam.Value} found");
        return;
      }

      var tutoring = tutorService.FindTutoring(tutorCode, exam.Value);

      if (tutoring == null)
      {
        // Tutoring doesn't exist
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
        response.WriteAsync($"no tutoring from tutor: {tutorCode} for exam: {exam.Value} found");
        return;
      }

      // Return tutoring from tutor for exam
      response.ContentType = "application/json; charset=utf-8";
      response.WriteAsync(JsonConvert.SerializeObject(tutoring));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static async void EndTutoringAction(int? id, int? duration, HttpResponse response,
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
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync("invalid request body");
          return;
        }

        if (durations == null)
        {
          // invalid request body
          response.StatusCode = StatusCodes.Status400BadRequest;
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync("invalid request body");
          return;
        }

        var exceedingDuration = durations.Find(x => x.Duration > GlobalConfig.BotConfig!.MaxTutoringDuration);
        if (exceedingDuration != default)
        {
          // invalid duration in request body
          response.StatusCode = StatusCodes.Status400BadRequest;
          response.ContentType = "text/html; charset=utf-8";
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
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync("invalid duration parameter");
        return;
      }

      if (tutorService.EndTutoring(id.Value, duration.Value))
        return;

      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"no tutoring with id {id.Value} found");
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static async void StartTutoringAction(int? tutorCode, int? studentCode, int? examCode,
    HttpResponse response,
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
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync("invalid request body");
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
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync("student code missing in url");
        return;
      }

      await StartTutoring(
        new TutorToStudentToExam(examCode.HasValue, tutorCode.Value, studentCode.Value, examCode),
        response);
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  private static async Task<bool> StartTutoring(TutorToStudentToExam tutoringData, HttpResponse response)
  {
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
    // Check if student is enabled
    if (!studentService.IsStudentEnabled(tutoringData.StudentCode))
    {
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"Student: {tutoringData.StudentCode} isn't enabled for tutoring");
      return false;
    }
    var tutor = tutorService.FindTutor(tutoringData.TutorCode);

    if (tutor!.Value.ContractState != 2) // 2 = signed contract
    {
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"Tutor: {tutoringData.TutorCode} doesn't have a signed contract");
      return false;
    }

    if (tutoringData.IsOFA)
    {
      // OFA tutoring
      // Check if tutor exists and is available for OFA
      if (tutor is not { OfaAvailable: true })
      {
        // Tutor isn't available for OFA or doesn't exist
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
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
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync(
          $"Tutoring from Tutor: {tutoringData.TutorCode} for Student: {tutoringData.StudentCode} is already active");
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
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"Tutor: {tutoringData.TutorCode} doesn't exist");
      return false;
    }

    if (tutoring.Value.AvailableTutorings < 1)
    {
      // tutoring doesn't have enough available tutorings
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"Tutor: {tutoringData.TutorCode} doesn't have enough available tutorings");
      return false;
    }

    // Start tutoring
    tutorService.ActivateTutoring(tutoringData.TutorCode, tutoringData.StudentCode, tutoringData.ExamCode.Value);
    return true;
  }
  
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static async void RemoveTutoring(HttpResponse response, HttpRequest request)
  {
    // Enable all given students
    List<TutorCodeToExamCode>? tutorings;
    try
    {
      tutorings = await request.ReadFromJsonAsync<List<TutorCodeToExamCode>>();
    }
    catch (Exception e)
    {
      // Invalid request body
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync("invalid request body");
      return;
    }

    if (tutorings == null)
    {
      // Invalid request body
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync("invalid request body");
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
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"Invalid tutorings in request body: {errorMessage}");
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
}

