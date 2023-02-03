using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

public static class CourseEndpoints
{
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static void FetchCourses(HttpResponse response)
  {
    try
    {
      var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
      response.ContentType = "application/json; charset=utf-8";
      response.WriteAsync(JsonConvert.SerializeObject(courseService.FindCourses()));
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }

  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static async void HandleCourseAction(string action, HttpResponse response, HttpRequest request)
  {
    if (action != "add" && action != "remove")
    {
      response.StatusCode = StatusCodes.Status404NotFound;
      return;
    }

    // Read request body
    List<Course>? courses;
    try
    {
      courses = await request.ReadFromJsonAsync<List<Course>>();
    }
    catch (Exception e)
    {
      // Invalid request body
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync("invalid request body");
      return;
    }

    if (courses == null)
    {
      // Invalid request body
      response.StatusCode = StatusCodes.Status400BadRequest;
      await response.WriteAsync("invalid request body");
      return;
    }

    try
    {
      var schoolService = new SchoolDAO(DbConnection.GetMySqlConnection());
      var schools = schoolService.FindSchools();

      var badCourse = courses.Find(x => !schools.Contains(x.School));

      if (badCourse != default)
      {
        // Courses list contains an invalid school
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsync($"invalid school for course: {badCourse.Name} in request body");
        return;
      }

      var courseService = new CourseDAO(DbConnection.GetMySqlConnection());

      switch (action)
      {
        case "add":
          if (courseService.AddCourse(courses, out var errorMessage)) return;
          response.StatusCode = StatusCodes.Status400BadRequest;
          await response.WriteAsync($"invalid course in request body: {errorMessage}");
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
  public static void DeleteCourses(HttpResponse response, HttpRequest request)
  {
    try
    {
      var courseService = new CourseDAO(DbConnection.GetMySqlConnection());

      courseService.DeleteCourses();
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
}