using System.Text.RegularExpressions;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;

namespace Bot.WebServer.EndPoints;

public static class ExamEndpoints
{
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static async void HandleExamAction(string action, HttpResponse response, HttpRequest request)
  {
    if (action != "add" && action != "remove")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    // Read request body
    List<Exam>? exams = null;
    try
    {
      exams = await request.ReadFromJsonAsync<List<Exam>>();
    }
    catch (Exception e)
    {
      // Invalid request body
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync("invalid request body");
      return;
    }

    if (exams == null)
    {
      // Invalid request body
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync("invalid request body");
      return;
    }

    var badCode = exams.Find(x => !Regex.IsMatch(x.Code.ToString(), RegularExpr.ExamCode));

    if (badCode != default)
    {
      // Student list contains an invalid exam code format
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync($"invalid exam code: {badCode.Code} in request body");
      return;
    }

    try
    {
      var examService = new ExamDAO(DbConnection.GetMySqlConnection());

      switch (action)
      {
        case "add":
          if (examService.AddExam(exams, out var errorMessage)) return;
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"invalid exams in request body: {errorMessage}");
          return;
        case "remove":
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
  
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static void DeleteExams(HttpResponse response, HttpRequest request)
  {
    try
    {
      var examService = new ExamDAO(DbConnection.GetMySqlConnection());

      examService.DeleteExams();
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
}