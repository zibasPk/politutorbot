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
    ///  Finds userId of user that locked Tutor,
    ///  </summary>
    /// <param name="tutor">Tutor to check.</param>
    ///  <param name="exam">Exam to check.</param>
    ///  <returns>Id of user that locked</returns>
    public long FindTutorLocker(string tutor, string exam)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam WHERE tutor=@tutor AND exam=@exam";
        var tutors = new List<Tutor>();
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
    
    /// <summary>
    /// Finds tutors.
    /// </summary>
    /// <returns>List of tutors.</returns>
    public List<Tutor> FindTutors()
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor";
        var tutors = new List<Tutor>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found");

            while (reader.Read())
            {
                var tutor = new Tutor
                {
                    Name = reader.GetString("name"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp")
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
    public List<Tutor> FindTutorsForExam(string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                             "WHERE exam=@exam AND lock_timestamp <= NOW() - INTERVAL @hours HOUR";
        var tutors = new List<Tutor>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@exam", exam);
            command.Parameters.AddWithValue("@hours", hoursSinceLock);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found for {exam} in db", exam);

            while (reader.Read())
            {
                var tutor = new Tutor
                {
                    Name = reader.GetString("name"),
                    Exam = reader.GetString("exam"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school"),
                    Ranking = reader.GetInt32("ranking"),
                    LockTimeStamp = reader.GetDateTime("lock_timestamp")
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
    public void LockTutor(string tutor, string exam, long user)
    {
        _connection.Open();
        const string query = "UPDATE tutor_to_exam SET lock_timestamp = NOW() AND locked_by=@user WHERE tutor=@tutor AND exam=@exam";
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
    /// Updates the lock_timestamp of the given tutor for the given exam to the 0000-00-00 00:00:00 DEFAULT timestamp.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    /// <param name="exam">Name of exam that needs to be unlocked.</param>
    public void UnlockTutor(string tutor, string exam)
    {
        _connection.Open();
        // start local transaction
        var transaction = _connection.BeginTransaction();
        const string query1 = "UPDATE telegram_user SET lock_timestamp = DEFAULT" +
                              "WHERE userID IN " +
                              "(select locked_by from tutor_to_exam where tutor=@tutor AND exam=@exam);";
        
        const string query2 = "UPDATE tutor_to_exam SET lock_timestamp = DEFAULT AND locked_by = DEFAULT WHERE tutor=@tutor AND exam=@exam";
        try
        {
            var command = new MySqlCommand(query1, _connection, transaction);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();
            command.ExecuteNonQuery();

            command.CommandText = query2;
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Parameters.AddWithValue("@exam", exam);
            command.Prepare();
            command.ExecuteNonQuery();
            
            transaction.Commit();
            Log.Debug("Tutor {tutor} was unlocked", tutor);
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
}