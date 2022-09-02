using Bot.Database.Entity;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class ReservationDAO
{
    private readonly MySqlConnection _connection;

    public ReservationDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    public Reservation? FindReservation(int reservationId)
    {
        _connection.Open();
        const string query = "SELECT * from reservation WHERE ID=@reservationId";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservation found in db with id: {0}", reservationId);

            if (reader.Read())
            {
                var reservation = new Reservation
                {
                    Id = reader.GetInt32("ID"),
                    Tutor = reader.GetInt32("tutor"),
                    Exam = reader.GetInt32("exam"),
                    ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                    IsProcessed = reader.GetBoolean("is_processed")
                };  
                _connection.Close();
                return reservation;
            }
           
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
        _connection.Close();
        return null;
    }
    
    public List<Reservation> FindReservations()
    {
        _connection.Open();
        const string query = "SELECT * from reservation";
        var reservations = new List<Reservation>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db");

            while (reader.Read())
            {
                var reservation = new Reservation
                {
                    Id = reader.GetInt32("ID"),
                    Tutor = reader.GetInt32("tutor"),
                    Exam = reader.GetInt32("exam"),
                    ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                    IsProcessed = reader.GetBoolean("is_processed")
                };
                reservations.Add(reservation);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
        _connection.Close();
        return reservations;
    }
    
    public List<Reservation> FindReservations(bool isProcessed)
    {
        _connection.Open();
        const string query = "SELECT * from reservation WHERE is_processed=@value";
        var reservations = new List<Reservation>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@value", isProcessed);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db with is_processed={0}",isProcessed);

            while (reader.Read())
            {
                var reservation = new Reservation
                {
                    Id = reader.GetInt32("ID"),
                    Tutor = reader.GetInt32("tutor"),
                    Exam = reader.GetInt32("exam"),
                    ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                    IsProcessed = reader.GetBoolean("is_processed")
                };
                reservations.Add(reservation);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
        _connection.Close();
        return reservations;
    }

    public bool IsReservationAllowed(int reservationId)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam as tut " +
                             "join (select * FROM reservation WHERE ID=@reservationId) as res " +
                             "on tut.exam=res.exam AND tut.tutor=res.tutor " +
                             "WHERE available_reservations > 0";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                Log.Debug("No available reservations with id: {0} found in db ", reservationId);
                _connection.Close();
                return false;
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
        _connection.Close();
        return true;
    }
}