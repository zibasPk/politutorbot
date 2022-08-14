using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class SchoolDAO
{
    private MySqlConnection _connection;

    public SchoolDAO(MySqlConnection connection)
    {
        _connection = connection;
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