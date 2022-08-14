using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class ExamDAO
{
    private MySqlConnection _connection;

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
        const string query = "SELECT * from exam WHERE year=@year AND course=@course";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@course", course);
        command.Prepare();

        var exams = new List<string>();
        MySqlDataReader reader;
        try
        {
            reader = command.ExecuteReader();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return exams;
        }

        if (!reader.HasRows)
        {
            Log.Debug("No exams found for {course} in year {year}", course, year);
            return exams;
        }

        while (reader.Read())
        {
            exams.Add(reader.GetString("name"));
        }

        return exams;
    }

    public List<string> FindSchools()
    {
        const string query = "SELECT * from school";
        var command = new MySqlCommand(query, _connection);

        var schools = new List<string>();
        MySqlDataReader reader;
        try
        {
            reader = command.ExecuteReader();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return schools;
        }

        if (!reader.HasRows)
        {
            Log.Debug("No schools found for in db");
            return schools;
        }

        while (reader.Read())
        {
            schools.Add(reader.GetString("name"));
        }

        return schools;
    }
    
}