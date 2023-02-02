using Bot.Database;
using Bot.Database.Dao;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bot.WebServer.EndPoints;

public static class ReservationEndpoints
{
  public static void FetchReservations(string? value, HttpResponse response)
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

      response.ContentType = "application/json; charset=utf-8";
      response.WriteAsync(returnObject);
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      response.StatusCode = StatusCodes.Status502BadGateway;
    }
  }
  
  public static void HandleReservationAction(int id, string action, HttpResponse response)
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
        response.ContentType = "text/html; charset=utf-8";
        response.WriteAsync($"no reservation with id: {id} found");
        return;
      }

      if (reservationService.IsReservationProcessed(id))
      {
        // Reservation already marked as processed
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = "text/html; charset=utf-8";
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
        response.ContentType = "text/html; charset=utf-8";
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
}