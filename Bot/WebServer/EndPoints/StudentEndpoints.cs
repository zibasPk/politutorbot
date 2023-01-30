using System.Text.RegularExpressions;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

public static class StudentEndpoints
{
  public static void FetchStudents(HttpResponse response)
  {
    try
    {
      var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
      response.ContentType = "application/json; charset=utf-8";
      response.WriteAsync(JsonConvert.SerializeObject(studentService.FindEnabledStudents()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
  
  public static async void HandleStudentAction(string action, int? studentCode, HttpResponse response,
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
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync($"invalid request body");
          return;
        }

        if (studentCodes == null)
        {
          // Invalid request body
          response.StatusCode = StatusCodes.Status400BadRequest;
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync($"invalid request body");
          return;
        }

        var badCode = studentCodes.Find(x => !Regex.IsMatch(x.ToString(), RegularExpr.StudentCode));

        if (badCode != default)
        {
          // Student list contains an invalid student code format
          response.StatusCode = StatusCodes.Status400BadRequest;
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync($"invalid student code: {badCode} in request body");
          return;
        }

        switch (action)
        {
          case "enable":
            if (studentService.EnableStudent(studentCodes)) return;
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.ContentType = "text/html; charset=utf-8";
            await response.WriteAsync($"duplicate student code in request body");
            return;
          case "disable":
            if (studentService.DisableStudent(studentCodes)) return;
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.ContentType = "text/html; charset=utf-8";
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
        response.ContentType = "text/html; charset=utf-8";
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
          response.ContentType = "text/html; charset=utf-8";
          await response.WriteAsync($"tried enabling already enabled student code: {studentCode.Value}");
          return;
        case "disable":
          if (studentService.DisableStudent(studentCode.Value))
            return;

          // Request contains a duplicate student code
          response.StatusCode = StatusCodes.Status400BadRequest;
          response.ContentType = "text/html; charset=utf-8";
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
}