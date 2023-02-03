using Bot.Database;
using Bot.Database.Dao;
using Bot.WebServer.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

public static class HistoryEndpoints
{
  [Authorize(AuthenticationSchemes = TokenAuthOptions.DefaultSchemeName)]
  public static void FetchHistory(string content, string? contentType, HttpResponse response)
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