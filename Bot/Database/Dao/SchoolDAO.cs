using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class SchoolDAO : DAO
{
    public SchoolDAO(MySqlConnection connection) : base(connection)
    {
    }

    /// <summary>
    /// Finds schools. 
    /// </summary>
    /// <returns>List of schools.</returns>
    public List<string> FindSchools()
    {
        Connection.Open();
        const string query = "SELECT * from school";
        var schools = new List<string>();
        try
        {
            var command = new MySqlCommand(query, Connection);

            var reader = command.ExecuteReader();

            if (!reader.HasRows)
                Log.Debug("No schools found in db");

            while (reader.Read())
                schools.Add(reader.GetString("name"));
        }
        catch (Exception)
        {
            Connection.Close();
            throw;
        }

        Connection.Close();
        return schools;
    }
}