using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class CourseDAO
{
    private readonly MySqlConnection _connection;

    public CourseDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Finds courses in db.
    /// </summary>
    /// <returns>List of courses in given school.</returns>
    public List<string> FindCourses()
    {
        _connection.Open();
        const string query = "SELECT * from course";
        var courses = new List<string>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No courses found in Db");

            while (reader.Read())
                courses.Add(reader.GetString("name"));
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return courses;
    }
    
    /// <summary>
    /// Finds courses in given school.
    /// </summary>
    /// <param name="school">The school in which to search.</param>
    /// <returns>List of courses in given school.</returns>
    public List<string> FindCourses(string school)
    {
        _connection.Open();
        const string query = "SELECT * from course WHERE school=@school";
        var courses = new List<string>();
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@school", school);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No courses found for school {school}", school);

            while (reader.Read())
                courses.Add(reader.GetString("name"));
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return courses;
    }

    /// <summary>
    /// Determines whether a School contains a specified Course 
    /// </summary>
    /// <param name="course">The Course to check</param> 
    /// <param name="school">The School in which to check</param> 
    /// <returns>true if the specified School contains the given Course; otherwise, false</returns>
    public bool IsCourseInSchool(string course, string school)
    {
        _connection.Open();
        const string query = "SELECT * from course WHERE name=@name";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", course);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var foundSchool = reader.GetString("school");
                if (foundSchool == school)
                {
                    _connection.Close();
                    return true;
                }

                Log.Debug("Course {course} is of School {foundSchool} not {school}."
                    , course, foundSchool, school);
            }
            else
            {
                Log.Debug("No courses found for school {school}", school);
            }
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