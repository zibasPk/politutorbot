using MySql.Data.MySqlClient;

namespace Bot.Database.Dao;

public class DAO
{
    protected readonly MySqlConnection Connection;

    protected DAO(MySqlConnection connection)
    {
        Connection = connection;
    }
}