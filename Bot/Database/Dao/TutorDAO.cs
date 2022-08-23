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

    /// <summary>
    /// Finds tutors, that aren't locked, for a specific exam.
    /// </summary>
    /// <param name="exam">Exam for which to find tutors.</param>
    /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>List of non locked tutors for exam.</returns>
    public List<Tutor> FindTutorsForExam(string exam, int hoursSinceLock)
    {
        _connection.Open();
        const string query = "SELECT * FROM tutor WHERE exam=@exam AND lock_timestamp <= NOW() - INTERVAL @hours HOUR";
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
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return tutors;
    }

    public bool IsTutorForExam(string tutor, string exam)
    {
        _connection.Open();
        const string query = "SELECT * from tutor WHERE name=@tutor";
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
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return false;
    }

    /// <summary>
    /// Updates the lock_timestamp of the given tutor to the current time.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    public void LockTutor(string tutor)
    {
        _connection.Open();
        const string query = "UPDATE tutor SET lock_timestamp = NOW() WHERE name=@tutor";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Prepare();

            command.ExecuteNonQuery();
            Log.Debug("Tutor {tutor} was locked", tutor);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Updates the lock_timestamp of the given tutor to the 0000-00-00 00:00:00 DEFAULT timestamp.
    /// </summary>
    /// <param name="tutor">Name of Tutor that needs to be updated.</param>
    public void UnlockTutor(string tutor)
    {
        _connection.Open();
        const string query = "UPDATE tutor SET lock_timestamp = DEFAULT WHERE name=@tutor";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@tutor", tutor);
            command.Prepare();

            command.ExecuteNonQuery();
            Log.Debug("Tutor {tutor} was unlocked", tutor);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }
}