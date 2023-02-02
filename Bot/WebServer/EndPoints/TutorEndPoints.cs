using System.Text.RegularExpressions;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

public static class TutorEndPoints
{
  public static void FetchTutors(HttpResponse response)
  {
    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      response.ContentType = "application/json; charset=utf-8";
      response.WriteAsync(JsonConvert.SerializeObject(tutorService.FindTutors()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  public static void HandleContractAction(int tutorCode, int state, HttpResponse response)
  {
    if (state is > 2 or < 0)
    {
      // Not a valid contract state
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
      response.WriteAsync($"Invalid state for contract with tutor: {tutorCode}");
      return;
    }

    try
    {
      var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
      if (tutorService.ChangeContractState(tutorCode, state))
        return;
      response.StatusCode = StatusCodes.Status502BadGateway;
      response.ContentType = "text/html; charset=utf-8";
      response.WriteAsync($"Error in contract state change for tutor: {tutorCode}");
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  public static async void HandleTutorAction(string action, HttpResponse response, HttpRequest request)
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
      response.ContentType = "text/html; charset=utf-8";
      await response.WriteAsync($"invalid tutor code: {badCode.TutorCode} in request body");
      return;
    }

    var badRanking = tutorings.Find(x => x.Ranking < 1);

    if (badRanking != default)
    {
      // Student list contains an invalid student code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      response.ContentType = "text/html; charset=utf-8";
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
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync($"invalid tutorings in request body: {errorMessage}");
          return;
        case "remove":
          // if (tutorService.AddTutor(tutorings[0])) return;
          // response.StatusCode = StatusCodes.Status400BadRequest;
          // response.ContentType = "application/json; charset=utf-8";
          response.StatusCode = StatusCodes.Status501NotImplemented;
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
  
  public static void DeleteTutors(HttpResponse response, HttpRequest request)
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
}