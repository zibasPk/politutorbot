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

    /// <summary>
    /// Fetches a complete reservation object from database. 
    /// </summary>
    /// <param name="reservationId">Id of the reservation to find.</param>
    /// <returns>A complete reservation object containing tutor name and surname in addition to the normal Reservation info.</returns>
    public ExtendedReservation? FindReservation(int reservationId)
    {
        _connection.Open();
        var transaction = _connection.BeginTransaction();
        const string query1 = "SELECT * from reservation WHERE ID=@reservationId";
        try
        {
            var command = new MySqlCommand(query1, _connection, transaction);
            command.Parameters.AddWithValue
                ("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                Log.Debug("No reservation found in db with id: {0}", reservationId);
                _connection.Close();
                return null;
            }

            var reservation = new ExtendedReservation
            {
                Id = reader.GetInt32("ID"),
                Tutor = reader.GetInt32("tutor"),
                Student = reader.GetInt32("student"),
                ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                IsProcessed = reader.GetBoolean("is_processed"),
                IsOFA = reader.GetBoolean("is_OFA")
            };
            var ordinal = reader.GetOrdinal("exam");
            if (!reader.IsDBNull(ordinal))
                reservation.Exam = reader.GetInt32("exam");


            command.CommandText = "SELECT * from tutor WHERE tutor_code=@tutor";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@tutor", reservation.Tutor);
            command.Prepare();
            reader = command.ExecuteReader();
            if (!reader.Read())
            {
                Log.Debug("While preparing extended reservation info, no tutor was found in db with code: {0}",
                    reservation.Tutor);
                _connection.Close();
                return null;
            }

            reservation.TutorName = reader.GetString("name");
            reservation.TutorName = reader.GetString("surname");
            transaction.Commit();
            _connection.Close();
            return reservation;
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
    }

    public List<ExtendedReservation> FindReservations()
    {
        _connection.Open();
        const string query = "SELECT * from reservation join tutor t on reservation.tutor = t.tutor_code";
        var reservations = new List<ExtendedReservation>();

        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db");

            while (reader.Read())
            {
                var ordinal = reader.GetOrdinal("name");
                if (!reader.IsDBNull(ordinal))
                    Log.Debug("While preparing extended reservation info, no tutor was found in db with code: {0}",
                        reader.GetInt32("tutor"));

                var reservation = new ExtendedReservation
                {
                    Id = reader.GetInt32("ID"),
                    Tutor = reader.GetInt32("tutor"),
                    TutorName = reader.GetString("name"),
                    TutorSurname = reader.GetString("surname"),
                    Student = reader.GetInt32("student"),
                    ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                    IsProcessed = reader.GetBoolean("is_processed"),
                    IsOFA = reader.GetBoolean("is_OFA")
                };
                ordinal = reader.GetOrdinal("exam");
                if (!reader.IsDBNull(ordinal))
                    reservation.Exam = reader.GetInt32("exam");
                
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

    public List<ExtendedReservation> FindReservations(bool isProcessed)
    {
        _connection.Open();
        const string query = "SELECT * from reservation join tutor t on reservation.tutor = t.tutor_code " +
                             "WHERE is_processed=@value";
        var reservations = new List<ExtendedReservation>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@value", isProcessed);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db with is_processed={0}", isProcessed);

            while (reader.Read())
            {
                var ordinal = reader.GetOrdinal("name");
                if (!reader.IsDBNull(ordinal))
                    Log.Debug("While preparing extended reservation info, no tutor was found in db with code: {0}",
                        reader.GetInt32("tutor"));
                
                var reservation = new ExtendedReservation
                {
                    Id = reader.GetInt32("ID"),
                    Tutor = reader.GetInt32("tutor"),
                    TutorName = reader.GetString("name"),
                    TutorSurname = reader.GetString("surname"),
                    Student = reader.GetInt32("student"),
                    ReservationTimestamp = reader.GetDateTime("reservation_timestamp"),
                    IsProcessed = reader.GetBoolean("is_processed"),
                    IsOFA = reader.GetBoolean("is_OFA")
                };
                ordinal = reader.GetOrdinal("exam");
                if (!reader.IsDBNull(ordinal))
                    reservation.Exam = reader.GetInt32("exam");
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
        try
        {
            var query = "SELECT * FROM reservation " +
                        "WHERE ID = @reservationId AND is_OFA = 1";
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                // checking if reservation regards an OFA tutoring
                Log.Debug("Reservation is for OFA");
                _connection.Close();
                return true;
            }

            reader.Close();

            query = "SELECT * FROM tutor_to_exam as tut " +
                    "join (select * FROM reservation WHERE ID=@reservationId) as res " +
                    "on tut.exam=res.exam AND tut.tutor=res.tutor " +
                    "WHERE available_tutorings > 0";
            command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();

            reader = command.ExecuteReader();
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

    public bool IsReservationProcessed(int reservationId)
    {
        _connection.Open();

        const string query = "SELECT * FROM reservation " +
                             "WHERE ID = @reservationId AND is_processed = 1";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                // checking if reservation regards an OFA tutoring
                Log.Debug("Reservation {0} is already processed", reservationId);
                _connection.Close();
                return true;
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return false;
    }
}