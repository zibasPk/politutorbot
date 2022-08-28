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

    ///  <summary>
    ///  Finds userId of user that locked a Tutor for a certain exam.
    ///  </summary>
    /// <param name="tutor">Tutor to check.</param>
    ///  <param name="exam">Exam to check.</param>
    ///  <returns>Id of user that locked</returns>
    public long FindTutorGatekeeper(string tutor, string exam)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam WHERE tutor=@tutor AND exam=@exam";
        var tutors = new List<Tutoring>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found");

            if (reader.Read())
            {
                var userId = reader.GetInt64("locked_by");
                _connection.Close();
                return userId;
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return 0;
    }

    public List<Tutoring> FindTutoring(string tutorName)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on tutor = name " +
                             "WHERE tutor=@name;";

        var tutors = new List<Tutoring>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", tutorName);
            command.Prepare();

            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                Log.Debug("No tutorings found for tutor {name} in db", tutorName);
            }
            else
            {
                while (reader.Read())
                {
                    var tutoring = new Tutoring
                    {
                        Name = reader.GetString("name"),
                        Course = reader.GetString("course"),
                        School = reader.GetString("school"),
                        Ranking = reader.GetInt32("ranking"),
                        Exam = reader.GetString("exam"),
                        LockTimeStamp = reader.GetDateTime("lock_timestamp"),
                        LockedBy = reader.GetInt64("locked_by")
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
    public List<Tutoring> FindTutors()
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor join tutor_to_exam";
        var tutors = new List<Tutoring>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found");

            while (reader.Read())
            {
                var tutor = new Tutoring
                {
                    Name = reader.GetString("name"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    Exam = reader.GetString("exam"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp"),
                    LockedBy = reader.GetInt64("locked_by")
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
    /// Finds tutors, for a specific exam.
    /// </summary>
    /// <param name="exam">Exam for which to find tutors.</param>
    /// <returns>List of tutors for exam.</returns>
    public List<Tutoring> FindTutors(string exam)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam";
        var tutors = new List<Tutoring>();
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
                var tutor = new Tutoring
                {
                    Name = reader.GetString("name"),
                    Exam = reader.GetString("exam"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp"),
                    LockedBy = reader.GetInt64("locked_by")
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
    public List<Tutoring> FindLockedTutors(string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam AND lock_timestamp >= NOW() - INTERVAL @hours HOUR";
        var tutors = new List<Tutoring>();
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
                var tutor = new Tutoring
                {
                    Name = reader.GetString("name"),
                    Exam = reader.GetString("exam"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp"),
                    LockedBy = reader.GetInt64("locked_by")
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
    /// Finds tutors, that aren't locked, for a specific exam.
    /// </summary>
    /// <param name="exam">Exam for which to find tutors.</param>
    /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>List of non locked tutors for exam.</returns>
    public List<Tutoring> FindUnlockedTutors(string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam AND lock_timestamp <= NOW() - INTERVAL @hours HOUR";
        var tutors = new List<Tutoring>();
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
                var tutor = new Tutoring
                {
                    Name = reader.GetString("name"),
                    Exam = reader.GetString("exam"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp"),
                    LockedBy = reader.GetInt64("locked_by")
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
    public bool IsTutorForExam(string tutor, string exam)
    {
        _connection.Open();
        const string query = "SELECT * from tutor join tutor_to_exam on name = tutor WHERE name=@tutor";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                var foundExam = reader.GetString("exam");
                _connection.Close();
                return exam.Equals(foundExam);
            }

            Log.Debug("Tutor {tutor} not found found in db", tutor);
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
    /// Updates the lock_timestamp of the given tutor for the given exam to the current time.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    /// <param name="exam">Name of exam that needs to be locked.</param>
    /// <param name="user">Name of user that locked the exam.</param>
    public void LockTutorAndUser(string tutor, string exam, long user)
    {
        //TODO: trasformare in una transazione con lock dell'utente e aggiornare doc
        _connection.Open();
        const string query =
            "UPDATE tutor_to_exam SET lock_timestamp = NOW(), locked_by=@user WHERE tutor=@tutor AND exam=@exam";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@user", user);
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

    /// <summary>
    /// Checks validity of values to insert for tutor.
    /// </summary>
    /// <returns>true if valid; otherwise false;</returns>
    public bool CheckTutorInsertValidity(Tutoring tutoring)
    {
        _connection.Open();
        try
        {
            var query = "SELECT * FROM course WHERE name=@course";
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@course", tutoring.Course);
            command.Prepare();

            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                Log.Debug("Invalid course param");
                _connection.Close();
                return false;
            }

            query = "SELECT * FROM school WHERE name=@school";
            command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@school", tutoring.School);
            command.Prepare();
            if (!reader.HasRows)
            {
                Log.Debug("Invalid school param");
                _connection.Close();
                return false;
            }

            query = "SELECT * FROM tutor WHERE ranking=@ranking";
            command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@ranking", tutoring.Ranking);
            command.Prepare();
            if (reader.HasRows)
            {
                Log.Debug("Invalid ranking param");
                _connection.Close();
                return false;
            }

            query = "SELECT * FROM exam WHERE name=@exam";
            command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@exam", tutoring.Exam);
            command.Prepare();
            if (!reader.HasRows)
            {
                Log.Debug("Invalid exam param");
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

    public void InsertNewTutor(Tutoring tutoring)
    {
        _connection.Open();
        var transaction = _connection.BeginTransaction();
        var query = "INSERT INTO tutor (`name`, `course`, `school`, `ranking`) " +
                    "VALUES (@name, @course, @school, @ranking)";
        try
        {
            var command = new MySqlCommand(query, _connection, transaction);
            try
            {
                command.Parameters.AddWithValue("@name", tutoring.Name);
                command.Parameters.AddWithValue("@course", tutoring.Course);
                command.Parameters.AddWithValue("@school", tutoring.School);
                command.Parameters.AddWithValue("@ranking", tutoring.Ranking);
                command.Prepare();
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                if (e.Code != 1022)
                    throw;
            }

            command.CommandText = "INSERT INTO tutor_to_exam (`tutor`, `exam`, `lock_timestamp`, `locked_by`) " +
                                  "VALUES (@name, @exam, DEFAULT, DEFAULT)";
            command.Parameters.AddWithValue("@name", tutoring.Name);
            command.Parameters.AddWithValue("@exam", tutoring.Exam);
            command.Prepare();
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Checks if tutor is locked for a certain exam.
    /// </summary>
    /// <param name="tutor">Tutor for which to check</param>
    /// <param name="exam">Exam for which to check.</param>
    /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>true if locked; otherwise false.</returns>
    public bool IsTutorLocked(string tutor, string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE tutor=@name AND exam=@exam AND lock_timestamp >= NOW() - INTERVAL @hours HOUR";
        var tutors = new List<Tutoring>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@hours", hoursSinceLock);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                Log.Debug("Tutors {tutor} isn't locked", tutor);
                _connection.Close();
                return false;
            }
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        Log.Debug("Tutors {tutor} is locked", tutor);
        _connection.Close();
        return true;
    }
}