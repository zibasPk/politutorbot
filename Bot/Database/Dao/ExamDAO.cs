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
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        if (reader != null)
        {
            if (!reader.HasRows)
                Log.Debug("No exams found for {course} in year {year}", course, year);

            while (reader.Read())
                exams.Add(reader.GetString("name"));
        }

        _connection.Close();
        return exams;
    }
}