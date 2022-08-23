using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class UserDAO
{
    // todo: riscrivere meglio le condizioni di return dei dao almeno i try catch
    private readonly MySqlConnection _connection;

    public UserDAO(MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Removes a row of 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>true removal is a success; otherwise false</returns>
    public bool RemoveUser(long userId)
    {
        _connection.Open();
        const string query = "DELETE FROM telegram_user WHERE userID=@userID";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userID", userId);
        command.Prepare();
        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return true;
    }

    /// <summary>
    /// Checks if user has already a saved personal code
    /// </summary>
    /// <param name="userId">Telegram userId of user for which to check</param>
    /// <returns>true user has a saved personal code; otherwise false.</returns>
    public bool IsUserLinked(long userId)
    {
        _connection.Open();
        const string query = "SELECT * from telegram_user WHERE userID=@userID";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userID", userId);
        command.Prepare();

        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                _connection.Close();
                return true;
            }

            Log.Debug("User {user} not found for in db", userId);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return false;
    }

    public int? FindUserStudentNumber(long userId)
    {
        _connection.Open();
        const string query = "SELECT * from telegram_user WHERE userID=@userID";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userID", userId);
        command.Prepare();

        MySqlDataReader? reader = null;
        try
        {
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                var result = reader.GetInt32("student_number");
                _connection.Close();
                return result;
            }

            Log.Debug("User {user} not found for in db", userId);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
        return null;
    }

    /// <summary>
    /// Saves a userId-studentNumber pair in DataBase
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="studentNumber"></param>
    /// <returns>false if insert fails; Otherwise true</returns>
    public void SaveUserLink(long userId, int studentNumber)
    {
        _connection.Open();
        const string query = "INSERT INTO telegram_user VALUES(@userID, @studentNumber, DEFAULT)";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userID", userId);
        command.Parameters.AddWithValue("@studentNumber", studentNumber);
        command.Prepare();
        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException e)
        {
            Log.Error(e.Message);
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Updates the lock_timestamp of the given user to the current time.
    /// </summary>
    /// <param name="userId">Telegram userId of user that needs to be updated.</param>
    public void LockUser(long userId)
    {
        _connection.Open();
        const string query = "UPDATE telegram_user SET lock_timestamp = NOW() WHERE userID=@userId";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Prepare();

        try
        {
            command.ExecuteNonQuery();
            Log.Debug("User {user} was locked", userId);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Updates the lock_timestamp of the given user to the 0000-00-00 00:00:00 DEFAULT timestamp.
    /// </summary>
    /// <param name="userId">Telegram userId of user that needs to be updated.</param>
    public void UnlockUser(long userId)
    {
        _connection.Open();
        const string query = "UPDATE telegram_user SET lock_timestamp = DEFAULT WHERE userID=@userId";
        var command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Prepare();

        try
        {
            command.ExecuteNonQuery();
            Log.Debug("User {user} was unlocked", userId);
        }
        catch (Exception e)
        {
            _connection.Close();
            throw;
        }

        _connection.Close();
    }

    /// <summary>
    /// Checks if the given user is locked.
    /// </summary>
    /// <param name="userId">Telegram userId to check.</param>
    /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
    /// <returns>true if user is locked; otherwise false.</returns>
    public bool IsUserLocked(long userId, int hoursSinceLock)
    {
        _connection.Open();
        const string query =
            "SELECT * FROM telegram_user WHERE userID=@userId AND lock_timestamp <= NOW() - INTERVAL @hours HOUR";
        try
        {
            var command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@hours", hoursSinceLock);
            command.Prepare();
            
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                Log.Debug("User {user} is currently locked", userId);
                _connection.Close();
                return true;
            }
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