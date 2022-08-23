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

    public List<Tutor> FindTutorsForExam(string exam)
    {
        _connection.Open();
        const string query = "SELECT * from tutor WHERE exam=@exam";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@exam", exam);
        command.Prepare();

        var tutors = new List<Tutor>();
        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No tutors found for {exam} in db", exam);

            while (reader.Read())
            {
                var tutor = new Tutor
                {
                    Name = reader.GetString("name"),
                    Exam = reader.GetString("exam"),
                    Course = reader.GetString("course"),
                    School = reader.GetString("school")
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
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@tutor", tutor);
        command.Prepare();

        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();

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
}