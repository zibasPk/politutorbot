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

    public List<string> FindSchools()
    {
        _connection.Open();
        const string query = "SELECT * from school";
        var command = new MySqlCommand(query, _connection);

        var schools = new List<string>();
        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        if (reader != null)
        {
            if (!reader.HasRows)
                Log.Debug("No schools found in db");

            while (reader.Read())
                schools.Add(reader.GetString("name"));
        }
        
        _connection.Close();
        return schools;
    }

}