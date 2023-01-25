using Bot.Database.Records;
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
    /// Finds exam by searching by its name and course.
    /// </summary>
    /// <param name="name">Name of exam to search.</param>
    /// <param name="course">The course for which to search.</param>
    /// <returns>Exam</returns>
    public Exam? FindExam(string name, string course)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE name=@name AND course=@course";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@course", course);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                Log.Debug("No exams found for {course} with name {name}", course, name);
                _connection.Close();
                return null;
            }

            var exam = new Exam
            {
                Code = reader.GetInt32("code"),
                Course = reader.GetString("course"),
                Name = reader.GetString("name"),
                Year = reader.GetString("year")
            };
            _connection.Close();
            return exam;
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }
    }
    
    /// <summary>
    /// Searches for an exam with a certain code.
    /// </summary>
    /// <param name="examCode"> Code of the exam</param>
    /// <returns>true if at least one exam with that name exists; false otherwise</returns>
    public bool FindExam(int examCode)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE code=@code";
        var exams = new List<string>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@code", examCode);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                Log.Debug("Exam {code} not found in db", examCode);
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
    
    /// <summary>
    /// Finds exams of a course from a specific year.
    /// </summary>
    /// <param name="course">The course for which to search.</param>
    /// <param name="year">The year for which to search.</param>
    /// <returns>List of exams of a course from a specific year.</returns>
    public List<Exam> FindExamsInYear(string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE year=@year AND course=@course";
        var exams = new List<Exam>();
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
            {
                var exam = new Exam
                {
                    Code = reader.GetInt32("code"),
                    Course = reader.GetString("course"),
                    Name = reader.GetString("name"),
                    Year = reader.GetString("year")
                };
                exams.Add(exam);
            }
        }
        catch (Exception)
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
    /// <param name="exam">The code of the exam for which to check.</param>
    /// <param name="course">The course for which to check.</param>
    /// <param name="year">The year for which to check.</param>
    /// <returns>true if exam is in course in year; otherwise false.</returns>
    public bool IsExamInCourse(int exam, string course, string year)
    {
        _connection.Open();
        const string query = "SELECT * from exam WHERE code=@code and course=@course and year=@year";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@code", exam);
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
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return false;
    }
}