using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class SchoolDAO
{
    private readonly MySqlConnection _connection;

    public SchoolDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Finds schools. 
    /// </summary>
    /// <returns>List of schools.</returns>
    public List<string> FindSchools()
    {
        _connection.Open();
        const string query = "SELECT * from school";
        var schools = new List<string>();
        try
        {
            var command = new MySqlCommand(query, _connection);

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No schools found in db");

            while (reader.Read())
                schools.Add(reader.GetString("name"));
        }
        catch (Exception)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return schools;
    }
}