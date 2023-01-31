using Bot.Database.Records;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class ReservationDAO : DAO
{
    public ReservationDAO(MySqlConnection connection) : base(connection)
    {
    }

    /// <summary>
    /// Fetches a complete reservation object from database. 
    /// </summary>
    /// <param name="reservationId">Id of the reservation to find.</param>
    /// <returns>A complete reservation object containing tutor name and surname in addition to the normal Reservation info.</returns>
    public ExtendedReservation? FindReservation(int reservationId)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        const string query1 = "SELECT * from reservation WHERE ID=@reservationId";
        try
        {
            var command = new MySqlCommand(query1, Connection, transaction);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                Log.Debug("No reservation found in db with id: {0}", reservationId);
                Connection.Close();
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

            reader.Close();

            command.Parameters.Clear();
            command.CommandText = "SELECT * from tutor WHERE tutor_code=@tutorCode";
            command.Parameters.AddWithValue("@tutorCode", reservation.Tutor);
            command.Prepare();
            reader = command.ExecuteReader();
            if (!reader.Read())
            {
                Log.Debug("While preparing extended reservation info, no tutor was found in db with code: {0}",
                    reservation.Tutor);
                Connection.Close();
                return null;
            }

            reservation.TutorName = reader.GetString("name");
            reservation.TutorSurname = reader.GetString("surname");
            reader.Close();
            transaction.Commit();
            Connection.Close();
            return reservation;
        }
        catch (Exception e)
        {
            Connection.Close();
            throw;
        }
    }

    public List<ExtendedReservation> FindReservations()
    {
        Connection.Open();
        const string query = "SELECT * from reservation join tutor t on reservation.tutor = t.tutor_code";
        var reservations = new List<ExtendedReservation>();

        try
        {
            var command = new MySqlCommand(query, Connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db");

            while (reader.Read())
            {
                var ordinal = reader.GetOrdinal("name");
                if (reader.IsDBNull(ordinal))
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
            Connection.Close();
            throw;
        }

        Connection.Close();
        return reservations;
    }

    public List<ExtendedReservation> FindReservations(bool isProcessed)
    {
        Connection.Open();
        const string query = "SELECT * from reservation join tutor t on reservation.tutor = t.tutor_code " +
                             "WHERE is_processed=@value";
        var reservations = new List<ExtendedReservation>();
        try
        {
            var command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@value", isProcessed);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No reservations found in db with is_processed={0}", isProcessed);

            while (reader.Read())
            {
                var ordinal = reader.GetOrdinal("name");
                if (reader.IsDBNull(ordinal))
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
            Connection.Close();
            throw;
        }

        Connection.Close();
        return reservations;
    }

    /// <summary>
    /// Will check if a reservation is allowed.
    /// </summary>
    /// <param name="reservationId">Id of the reservation to check.</param>
    /// <returns>False if reservation isn't allowed, true otherwise.</returns>
    public bool IsReservationAllowed(int reservationId)
    {
        Connection.Open();
        try
        {
            var query = "SELECT * FROM reservation " +
                        "WHERE ID = @reservationId AND is_OFA = 1";
            var command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                // checking if reservation regards an OFA tutoring
                Log.Debug("Reservation {0} is for OFA tutoring", reservationId);
                Connection.Close();
                return true;
            }

            reader.Close();

            query = "SELECT * FROM tutor_to_exam as tut " +
                    "join (select * FROM reservation WHERE ID=@reservationId) as res " +
                    "on tut.exam=res.exam AND tut.tutor=res.tutor " +
                    "WHERE available_tutorings > 0";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();

            reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                // checking if reservation is for tutoring with aviable tutoring
                Log.Debug("No available reservations with id: {0} found in db ", reservationId);
                Connection.Close();
                return false;
            }
        }
        catch (Exception)
        {
            Connection.Close();
            throw;
        }

        Connection.Close();
        return true;
    }

    public bool IsReservationProcessed(int reservationId)
    {
        Connection.Open();

        const string query = "SELECT * FROM reservation " +
                             "WHERE ID = @reservationId AND is_processed = 1";
        try
        {
            var command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                // checking if reservation regards an OFA tutoring
                Log.Debug("Reservation {0} is already processed", reservationId);
                reader.Close();
                Connection.Close();
                return true;
            }

            reader.Close();
        }
        catch (Exception)
        {
            Connection.Close();
            throw;
        }

        Connection.Close();
        return false;
    }

    /// <summary>
    /// Updates user lock time stamp, if the reservation isn't for OFA updates last reservation time stamp in tutor_to_exam
    /// table and sets reservation as processed.
    /// </summary>
    /// <param name="id">Id of reservation to refuse</param>
    public void RefuseReservation(int id)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        const string query1 = "UPDATE reservation SET is_processed = 1 " +
                              "WHERE ID=@reservationId";
        const string query2 = "SELECT * FROM reservation WHERE ID=@reservationId";
        const string query3 = "UPDATE tutor_to_exam SET last_reservation = DEFAULT " +
                              "WHERE tutor=@tutorCode AND exam=@examCode";
        const string query4 = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                              "WHERE student_code=@studentCode";
        try
        {
            var command = new MySqlCommand(query1, Connection, transaction);
            command.Parameters.AddWithValue("@reservationId", id);
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = query2;
            command.Prepare();
            var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                Log.Warning("Tried refusing non-existent reservation with id: {0}", id);
                Connection.Close();
                return;
            }

            var tutorCode = reader.GetInt32("tutor");
            var studentCode = reader.GetInt32("student");
            if (!reader.GetBoolean("is_OFA"))
            {
                var examCode = reader.GetInt32("exam");
                reader.Close();
                command.CommandText = query3;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@tutorCode", tutorCode);
                command.Parameters.AddWithValue("@examCode", examCode);
                command.Prepare();
                command.ExecuteNonQuery();
            }

            reader.Close();
            command.CommandText = query4;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@studentCode", studentCode);
            command.Prepare();
            command.ExecuteNonQuery();
            Log.Warning("Reservation with id: {0} was refused", id);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            Connection.Close();
            throw;
        }
    }
}