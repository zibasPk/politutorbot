using Bot.Database.Entity;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class TutorDAO
{
    private readonly MySqlConnection _connection;

    public TutorDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    public int? FindTutorCode(string tutorFullName, int exam)
    {
        var names = tutorFullName.Split(' ');
        if (names.Length < 2)
        {
            Log.Debug("Failed to properly split name: {t}", tutorFullName);
            return null;
        }

        var firstName = names[0];
        var lastName = names[1];
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                             "WHERE name=@firstName AND surname=@lastName AND exam=@exam;";

        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                Log.Debug("No tutor code found for {name} in db", tutorFullName);
                _connection.Close();
                return null;
            }

            var tudorCode = reader.GetInt32("tutor");
            _connection.Close();
            return tudorCode;
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
    }

    public TutorToExam? FindTutoring(int tutor, int exam)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                             "WHERE tutor=@tutor AND exam=@exam;";

        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                Log.Debug("No tutoring found with code {0} for exam {1} in db", tutor, exam);
                _connection.Close();
                return null;
            }

            var tutorObj = new TutorToExam
            {
                TutorCode = reader.GetInt32("tutor"),
                Name = reader.GetString("name"),
                Surname = reader.GetString("surname"),
                ExamCode = reader.GetInt32("exam"),
                Professor = reader.GetString("exam_professor"),
                Course = reader.GetString("course"),
                Ranking = reader.GetInt32("ranking"),
                OfaAvailable = reader.GetBoolean("OFA_available"),
                LastReservation = reader.GetDateTime("last_reservation"),
                AvailableReservations = reader.GetInt32("available_reservations")
            };
            _connection.Close();
            return tutorObj;
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
    }

    public List<TutorToExam> FindTutorings(int tutorCode)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                             "WHERE tutor=@code;";

        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@code", tutorCode);
            command.Prepare();

            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                Log.Debug("No tutorings found for tutor {name} in db", tutorCode);
            }
            else
            {
                while (reader.Read())
                {
                    var tutoring = new TutorToExam
                    {
                        TutorCode = reader.GetInt32("tutor"),
                        Name = reader.GetString("name"),
                        Surname = reader.GetString("surname"),
                        ExamCode = reader.GetInt32("exam"),
                        Professor = reader.GetString("exam_professor"),
                        Course = reader.GetString("course"),
                        Ranking = reader.GetInt32("ranking"),
                        OfaAvailable = reader.GetBoolean("OFA_available"),
                        LastReservation = reader.GetDateTime("last_reservation"),
                        AvailableReservations = reader.GetInt32("available_reservations")
                    };
                    tutors.Add(tutoring);
                }
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    /// <summary>
    /// Finds tutors.
    /// </summary>
    /// <returns>List of tutors.</returns>
    public List<TutorToExam> FindTutorings()
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor join tutor_to_exam";
        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found");

            while (reader.Read())
            {
                var tutor = new TutorToExam
                {
                    TutorCode = reader.GetInt32("tutor"),
                    Name = reader.GetString("name"),
                    Surname = reader.GetString("surname"),
                    ExamCode = reader.GetInt32("exam"),
                    Professor = reader.GetString("exam_professor"),
                    Course = reader.GetString("course"),
                    Ranking = reader.GetInt32("ranking"),
                    OfaAvailable = reader.GetBoolean("OFA_available"),
                    LastReservation = reader.GetDateTime("last_reservation"),
                    AvailableReservations = reader.GetInt32("available_reservations")
                };
                tutors.Add(tutor);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    /// <summary>
    /// Finds tutoring, for a specific exam.
    /// </summary>
    /// <param name="exam">Exam for which to find tutoring.</param>
    /// <returns>List of tutorings for exam.</returns>
    public List<TutorToExam> FindTutorings(string exam)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam";
        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found for {exam} in db", exam);

            while (reader.Read())
            {
                var tutor = new TutorToExam
                {
                    TutorCode = reader.GetInt32("tutor"),
                    Name = reader.GetString("name"),
                    Surname = reader.GetString("surname"),
                    ExamCode = reader.GetInt32("exam"),
                    Professor = reader.GetString("exam_professor"),
                    Course = reader.GetString("course"),
                    Ranking = reader.GetInt32("ranking"),
                    OfaAvailable = reader.GetBoolean("OFA_available"),
                    LastReservation = reader.GetDateTime("last_reservation"),
                    AvailableReservations = reader.GetInt32("available_reservations")
                };
                tutors.Add(tutor);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    /// <summary>
    /// Finds tutors, that are locked, for a specific exam.
    /// </summary>
    /// <param name="exam">Exam for which to find tutors.</param>
    /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>List of locked tutors for exam.</returns>
    public List<TutorToExam> FindLockedTutors(string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam AND lock_timestamp >= NOW() - INTERVAL @hours HOUR";
        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@hours", hoursSinceLock);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No unlocked tutors found for {exam} in db", exam);

            while (reader.Read())
            {
                var tutor = new TutorToExam
                {
                    TutorCode = reader.GetInt32("tutor"),
                    Name = reader.GetString("name"),
                    Surname = reader.GetString("surname"),
                    ExamCode = reader.GetInt32("exam"),
                    Professor = reader.GetString("exam_professor"),
                    Course = reader.GetString("course"),
                    Ranking = reader.GetInt32("ranking"),
                    OfaAvailable = reader.GetBoolean("OFA_available"),
                    LastReservation = reader.GetDateTime("last_reservation"),
                    AvailableReservations = reader.GetInt32("available_reservations")
                };
                tutors.Add(tutor);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    /// <summary>
    /// Finds tutors, that are available, for a specific exam.
    /// A tutor is available if it hasn't had a reservation in the last 24 hours and if it has
    /// </summary>
    /// <param name="exam">The code of the exam for which to find tutors.</param>
    /// <param name="lockHours">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>List of non locked tutors for exam.</returns>
    public List<TutorToExam> FindAvailableTutors(int exam, int lockHours)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code=tutor " +
                             "WHERE exam=@exam AND last_reservation <= NOW() - INTERVAL @hours HOUR " +
                             "AND available_reservations > 0";
        var tutors = new List<TutorToExam>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@hours", lockHours);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No unlocked tutors found for {exam} in db", exam);

            while (reader.Read())
            {
                var tutor = new TutorToExam
                {
                    TutorCode = reader.GetInt32("tutor"),
                    Name = reader.GetString("name"),
                    Surname = reader.GetString("surname"),
                    ExamCode = reader.GetInt32("exam"),
                    Professor = reader.GetString("exam_professor"),
                    Course = reader.GetString("course"),
                    Ranking = reader.GetInt32("ranking"),
                    OfaAvailable = reader.GetBoolean("OFA_available"),
                    LastReservation = reader.GetDateTime("last_reservation"),
                    AvailableReservations = reader.GetInt32("available_reservations")
                };
                tutors.Add(tutor);
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    /// <summary>
    /// Checks if if a tutor teaches a given exam.
    /// </summary>
    /// <param name="tutor">The tutor for which to check.</param>
    /// <param name="exam">The exam to check.</param>
    /// <returns>true if the tutor teaches the exam; otherwise false.</returns>
    public bool IsTutorForExam(int tutor, int exam)
    {
        _connection.Open();
        const string query =
            "SELECT * from tutor join tutor_to_exam on tutor_code=tutor WHERE tutor_code=@tutor AND exam=@exam;";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                _connection.Close();
                return true;
            }

            Log.Debug("Tutor {tutor} not found found for exam {exam} in db", tutor, exam);
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return false;
    }

    /// <summary>
    /// Updates last reservation time stamp in tutor_to_exam table, updated user lock time stamp
    /// <br/> and inserts reservation into reservation table.
    /// </summary>
    /// <param name="tutor">Student code of Tutor that needs to be reserved.</param>
    /// <param name="exam">Code of exam that needs to be reserved.</param>
    /// <param name="user">Telegram ID of user that reserved the tutor.</param>
    /// <param name="studentCode">StudentCode of the user that made the reservation</param>
    public void ReserveTutor(int tutor, int exam, long user, int studentCode)
    {
        //TODO: trasformare in una transazione con lock dell'utente e aggiornare doc
        _connection.Open();
        var transaction = _connection.BeginTransaction();

        try
        {
            const string query =
                "UPDATE tutor_to_exam SET last_reservation = NOW()" +
                "WHERE tutor=@tutor AND exam=@exam";
            var command = new MySqlCommand(query, _connection, transaction);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();
            command.ExecuteNonQuery();

            command.Parameters.Clear();
            command.CommandText = "UPDATE telegram_user SET lock_timestamp = NOW() "
                                  + "WHERE userID = @userID";
            command.Parameters.AddWithValue("@userID", user);
            command.Prepare();
            command.ExecuteNonQuery();

            command.Parameters.Clear();
            command.CommandText = "INSERT INTO reservation (tutor,exam,student) VALUES (@tutor,@exam,@student)";
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@student", studentCode);
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.Commit();
            Log.Debug("Tutor {tutor} was reserved", tutor);
        }
        catch (Exception e)
        {
            Log.Error("Exception while user {0} with student code {1} was reserving tutor {2} for exam {3}"
                , user, studentCode, tutor, exam);
            transaction.Rollback();
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Updates the lock_timestamp of the given tutor for the given exam to the current time.<br/>
    /// This Method is only to be called by the web api.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    /// <param name="exam">Name of exam that needs to be locked.</param>
    public void LockTutor(string tutor, string exam)
    {
        _connection.Open();
        const string query =
            "UPDATE tutor_to_exam SET lock_timestamp = NOW(), locked_by=DEFAULT WHERE tutor=@tutor AND exam=@exam";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            command.ExecuteNonQuery();
            Log.Debug("Tutor {tutor} was locked", tutor);
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Updates the lock_timestamp of the given tutor for the given exam to the 0000-00-00 00:00:00 DEFAULT timestamp.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    /// <param name="exam">Name of exam that needs to be unlocked.</param>
    public void UnlockTutor(string tutor, string exam)
    {
        _connection.Open();
        // start local transaction
        var transaction = _connection.BeginTransaction();
        const string query1 = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                              "WHERE userID IN " +
                              "(select locked_by from tutor_to_exam where tutor=@tutor AND exam=@exam)";

        const string query2 = "UPDATE tutor_to_exam SET lock_timestamp = DEFAULT ,locked_by = DEFAULT " +
                              "WHERE tutor=@tutor AND exam=@exam;";
        try
        {
            var command = new MySqlCommand(query1, _connection, transaction);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = query2;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.Commit();
            Log.Debug("Tutor {tutor} and gatekeeper user were unlocked", tutor);
        }
        catch (Exception)
        {
            // Attempt to roll back the transaction.
            try
            {
                transaction.Rollback();
            }
            catch (Exception ex2)
            {
                // This catch block will handle any errors that may have occurred
                // on the server that would cause the rollback to fail, such as
                // a closed connection.
                Log.Error("Rollback Exception Type: {0}", ex2.GetType());
                Log.Error("  Message: {0}", ex2.Message);
            }

            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Deletes a tutor from db.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be deleted.</param>
    public void DeleteTutor(string tutor)
    {
        _connection.Open();
        const string query = "DELETE FROM tutor WHERE (`name` = @tutor)";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Prepare();

            command.ExecuteNonQuery();
            Log.Debug("Tutor {tutor} was deleted", tutor);
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }


    public void ActivateTutoring(int reservationId)
    {
        _connection.Open();
        var transaction = _connection.BeginTransaction();
        const string query =
            "UPDATE tutor_to_exam as t SET available_reservations = available_reservations - 1, last_reservation = DEFAULT " +
            "WHERE EXISTS (select * FROM reservation as res WHERE ID=@reservationId AND " +
            "res.exam = t.exam AND res.tutor = t.tutor);";
        try
        {
            var command = new MySqlCommand(query, _connection, transaction);
            command.Parameters.AddWithValue("@reservationId", reservationId);
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = "UPDATE reservation SET is_processed = 1 " +
                                  "WHERE ID=@reservationId;";
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                                  "WHERE student_code IN (select student FROM reservation WHERE ID=@reservationId);";
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO active_tutoring (tutor, exam, student) " +
                                  "SELECT tutor, exam, student " +
                                  "FROM reservation " +
                                  "WHERE ID=@reservationId;";
            command.Prepare();
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    public bool DeactivateTutoring(int tutor, int exam, int studentCode)
    {
        _connection.Open();
        var transaction = _connection.BeginTransaction();
        const string query =
            "DELETE FROM active_tutoring WHERE tutor = @tutor AND exam = @exam AND student = @studentCode";
        try
        {
            var command = new MySqlCommand(query, _connection, transaction);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@studentCode", studentCode);
            command.Prepare();
            var affectedRows = command.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                transaction.Commit();
                _connection.Close();
                return false;
            }
            
            command.CommandText = "UPDATE tutor_to_exam SET available_reservations = available_reservations + 1 " +
                                  "WHERE exam=@exam AND tutor=@tutor";
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.Commit();
            Log.Debug("Tutoring from tutor: {tutor} to student: {studentCode} was deleted", tutor, studentCode);
        }
        catch (Exception)
        {
            transaction.Rollback();
            _connection.Close();
            throw;
        }

        _connection.Close();
        return true;
    }
}