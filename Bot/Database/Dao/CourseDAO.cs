using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class CourseDAO
{
    private MySqlConnection _connection;

    public CourseDAO(MySqlConnection connection)
    {
        _connection = connection;
    }
    
    public List<string> FindCoursesInSchool(string school)
    {
        const string query = "SELECT * from exam WHERE school=@school";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@year", school);
        command.Prepare();

        var courses = new List<string>();
        MySqlDataReader reader;
        try
        {
            reader = command.ExecuteReader();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return courses;
        }

        if (!reader.HasRows)
        {
            Log.Debug("No courses found for school {school}", school);
            return courses;
        }

        while (reader.Read())
        {
            courses.Add(reader.GetString("name"));
        }

        return courses;
    }
}