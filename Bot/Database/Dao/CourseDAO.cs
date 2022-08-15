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

    public List<string> FindCoursesInSchool(string school)
    {
        _connection.Open();
        const string query = "SELECT * from course WHERE school=@school";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@school", school);
        command.Prepare();

        var courses = new List<string>();
        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();
        }
        catch (Exception e)
        {
            _connection.Close();
            Log.Error(e.Message);
        }

        if (reader != null)
        {
            if (!reader.HasRows)
                Log.Debug("No courses found for school {school}", school);
            
            while (reader.Read())
                courses.Add(reader.GetString("name"));
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
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@name", course);
        command.Prepare();

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
            if (reader.Read())
            {
                var foundSchool = reader.GetString("school");
                if (foundSchool == school)
                    return true;
                Log.Debug("Course {course} is of School {foundSchool} not {school}."
                    , course, foundSchool, school);
            }
            else
            {
                Log.Debug("No courses found for school {school}", school);
            }
        }

        _connection.Close();
        return false;
    }
}