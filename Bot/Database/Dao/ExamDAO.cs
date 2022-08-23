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
    /// <param name="course">The course for which to search.</param>
    /// <param name="year">The year for which to search.</param>
    /// <returns>List of exams of a course from a specific year.</returns>
    public List<string> FindExamsInYear(string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE year=@year AND course=@course";
        var exams = new List<string>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@year", year);
            command.Parameters.AddWithValue("@course", course);
            command.Prepare();

            var reader = command.ExecuteReader();

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

    /// <summary>
    /// Checks if the given exam is in the given course and given year.
    /// </summary>
    /// <param name="exam">The exam for which to check.</param>
    /// <param name="course">The course for which to check.</param>
    /// <param name="year">The year for which to check.</param>
    /// <returns>true if exam is in course in year; otherwise false.</returns>
    public bool IsExamInCourse(string exam, string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE name=@name and course=@course and year=@year";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", exam);
            command.Parameters.AddWithValue("@course", course);
            command.Parameters.AddWithValue("@year", year);
            command.Prepare();

            var reader = command.ExecuteReader();

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