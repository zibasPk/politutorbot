using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class ExamDAO
{
    private readonly MySqlConnection _connection;

    public ExamDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Finds exams of a course from a specific year.
    /// </summary>
    /// <param name="course"></param>
    /// <param name="year"></param>
    /// <returns>List of exams of a course from a specific year.</returns>
    public List<string> FindExamsInYear(string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE year=@year AND course=@course";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@course", course);
        command.Prepare();

        var exams = new List<string>();
        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No exams found for {course} in year {year}", course, year);

            while (reader.Read())
                exams.Add(reader.GetString("name"));
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return exams;
    }

    public bool IsExamInCourse(string exam, string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE name=@name and course=@course and year=@year";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@name", exam);
        command.Parameters.AddWithValue("@course", course);
        command.Parameters.AddWithValue("@year", year);
        command.Prepare();

        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                _connection.Close();
                return true;
            }

            Log.Debug("{exam} {course} {year} doesn't exist", exam, course, year);
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