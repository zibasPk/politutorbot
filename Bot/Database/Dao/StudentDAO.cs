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
    const string query = "SELECT * from enabled_student WHERE student_code=@code";
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
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return false;
  }

  /// <summary>
  /// Returns all enabled students.
  /// </summary>
  /// <returns>List of student codes of enabled students. </returns>
  public List<int> FindEnabledStudents()
  {
    _connection.Open();
    const string query = "SELECT * from enabled_student";
    try
    {
      var tempList = new List<int>();

      var command = new MySqlCommand(query, _connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
      {
        Log.Debug("No enabled students found in DB");
      }

      while (reader.Read())
      {
        tempList.Add(reader.GetInt32("student_code"));
      }

      reader.Close();

      _connection.Close();
      return tempList;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Enables given students to use tutor bot.
  /// </summary>
  /// <param name="studentCodes">Student codes of students to enable.</param>
  /// <returns>false if one of the student codes is a duplicate entry, otherwise true.</returns>
  public bool EnableStudent(List<int> studentCodes)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query = "INSERT INTO enabled_student VALUES (@studentCode)";

    var i = 0;
    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      for (i = 0; i < studentCodes.Count; i++)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@studentCode", studentCodes[i]);
        command.ExecuteNonQuery();
      }

      transaction.Commit();
      _connection.Close();
      return true;
    }
    catch (Exception e)
    {
      if (e is MySqlException { Number: 1062 })
      {
        // duplicate key entry
        Log.Warning($"Duplicate key entry for an enabled student: {studentCodes[i]}");
        _connection.Close();
        return false;
      }

      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Enables given student to use tutor bot.
  /// </summary>
  /// <param name="studentCode">Student code of student to enable.</param>
  /// <returns>false if the student code is a duplicate entry, otherwise true.</returns>
  public bool EnableStudent(int studentCode)
  {
    _connection.Open();
    const string query = "INSERT INTO enabled_student VALUES (@studentCode)";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@studentCode", studentCode);
      command.ExecuteNonQuery();

      _connection.Close();
      return true;
    }
    catch (Exception e)
    {
      if (e is MySqlException { Number: 1062 })
      {
        // duplicate key entry
        _connection.Close();
        Log.Warning($"Duplicate key entry for enabled student: {studentCode}");
        return false;
      }

      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Disables given students from using the tutor bot.
  /// </summary>
  /// <param name="studentCodes">Student codes of students to disable.</param>
  /// <returns>false if one the student codes doesn't exist, otherwise true.</returns>
  public bool DisableStudent(List<int> studentCodes)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query = "DELETE FROM enabled_student where student_code = @studentCode";

    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      foreach (var studentCode in studentCodes)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@studentCode", studentCode);
        if (command.ExecuteNonQuery() != 0)
          continue;
        Log.Warning($"Tried disabling non-enabled student: {studentCode}");
        _connection.Close();
        return false;
      }

      transaction.Commit();
      _connection.Close();
      return true;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Disables a given student from using the tutor bot.
  /// </summary>
  /// <param name="studentCode">Student code of student to disable.</param>
  /// <returns>false if the student code doesn't exist, otherwise true.</returns>
  public bool DisableStudent(int studentCode)
  {
    _connection.Open();
    const string query = "DELETE FROM enabled_student where student_code = @studentCode";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@studentCode", studentCode);
      if (command.ExecuteNonQuery() == 0)
      {
        Log.Warning($"Tried disabling non-enabled student: {studentCode}");
        _connection.Close();
        return false;
      }

      _connection.Close();
      return true;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }
}