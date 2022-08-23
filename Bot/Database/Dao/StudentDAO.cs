using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class StudentDAO
{
    private readonly MySqlConnection _connection;

    public StudentDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Checks if the given student is enabled for tutoring.
    /// </summary>
    /// <param name="studentCode">Student Code of the student for which to check.</param>
    /// <returns>true if student is enabled; otherwise false.</returns>
    public bool IsStudentEnabled(int studentCode)
    {
        _connection.Open();
        const string query = "SELECT * from enabled_student WHERE code=@code";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@code", studentCode);
            command.Prepare();

            var reader = command.ExecuteReader();

            if (reader is { HasRows: true })
            {
                _connection.Close();
                return true;
            }

            Log.Debug("Student {code} not found in DB", studentCode);
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