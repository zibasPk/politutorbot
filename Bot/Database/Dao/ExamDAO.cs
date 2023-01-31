using Bot.Database.Records;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class ExamDAO : DAO
{
  public ExamDAO(MySqlConnection connection) : base(connection)
  {
  }
  
  /// <summary>
  /// Deletes all exams from the db
  /// </summary>
  /// <returns>true if deletion worked, otherwise false.</returns>
  public void DeleteExams()
  {
    Connection.Open();
    const string query = "DELETE FROM exam";

    try
    {
      var command = new MySqlCommand(query, Connection);
      command.ExecuteNonQuery();
      Log.Debug("All exams where deleted from db");
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Adds new exams to db.
  /// </summary>
  /// <param name="exams">exams to add</param>
  /// <param name="errorMessage">error message</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddExam(List<Exam> exams, out string errorMessage)
  {
    errorMessage = "";
    Connection.Open();
    const string query = "INSERT INTO exam (code,course,name,year) " +
                         "VALUES (@examCode,@course,@examName,@year)";
    try
    {
      var command = new MySqlCommand(query, Connection);


      foreach (var exam in exams)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@examCode", exam.Code);
        command.Parameters.AddWithValue("@course", exam.Course);
        command.Parameters.AddWithValue("@examName", exam.Name);
        command.Parameters.AddWithValue("@year", exam.Year);
        try
        {
          command.Prepare();
          command.ExecuteNonQuery();
        }
        catch (Exception e)
        {
          switch (e)
          {
            case MySqlException { Number: 1062 }:
              // duplicate key entry
              errorMessage = $"Duplicate entry for exam: {exam.Code} for course {exam.Course}";
              Log.Debug(errorMessage);
              break;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              Connection.Close();
              errorMessage =
                $"Adding new exam: {exam.Code} with non-existing course: {exam.Course}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              Connection.Close();
              errorMessage =
                $"Tried adding new exam: {exam.Code} {exam.Course} with an empty value";
              Log.Warning(errorMessage);
              return false;
            default:
              throw;
          }
        }
      }

      Connection.Close();
      return true;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Adds new exams to db.
  /// </summary>
  /// <param name="exam">exam to add</param>
  /// <param name="errorMessage">error message</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddExam(Exam exam, out string errorMessage)
  {
    var exams = new List<Exam> { exam };
    return AddExam(exams, out errorMessage);
  }

  /// <summary>
  /// Finds exam by searching by its name and course.
  /// </summary>
  /// <param name="name">Name of exam to search.</param>
  /// <param name="course">The course for which to search.</param>
  /// <returns>Exam</returns>
  public Exam? FindExam(string name, string course)
  {
    Connection.Open();
    const string query = "SELECT * from exam WHERE name=@name AND course=@course";
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@name", name);
      command.Parameters.AddWithValue("@course", course);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.Read())
      {
        Log.Debug("No exams found for {course} with name {name}", course, name);
        Connection.Close();
        return null;
      }

      var exam = new Exam
      {
        Code = reader.GetInt32("code"),
        Course = reader.GetString("course"),
        Name = reader.GetString("name"),
        Year = reader.GetString("year")
      };
      Connection.Close();
      return exam;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Searches for an exam with a certain code.
  /// </summary>
  /// <param name="examCode"> Code of the exam</param>
  /// <returns>true if at least one exam with that name exists; false otherwise</returns>
  public bool FindExam(int examCode)
  {
    Connection.Open();
    const string query = "SELECT * from exam WHERE code=@code";
    var exams = new List<string>();
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@code", examCode);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
      {
        Log.Debug("Exam {code} not found in db", examCode);
        Connection.Close();
        return false;
      }
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();
    return true;
  }

  /// <summary>
  /// Finds exams of a course from a specific year.
  /// </summary>
  /// <param name="course">The course for which to search.</param>
  /// <param name="year">The year for which to search.</param>
  /// <returns>List of exams of a course from a specific year.</returns>
  public List<Exam> FindExamsInYear(string course, string year)
  {
    Connection.Open();
    const string query = "SELECT * from exam WHERE year=@year AND course=@course";
    var exams = new List<Exam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@year", year);
      command.Parameters.AddWithValue("@course", course);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No exams found for {course} in year {year}", course, year);

      while (reader.Read())
      {
        var exam = new Exam
        {
          Code = reader.GetInt32("code"),
          Course = reader.GetString("course"),
          Name = reader.GetString("name"),
          Year = reader.GetString("year")
        };
        exams.Add(exam);
      }
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();
    return exams;
  }

  /// <summary>
  /// Checks if the given exam is in the given course and given year.
  /// </summary>
  /// <param name="exam">The code of the exam for which to check.</param>
  /// <param name="course">The course for which to check.</param>
  /// <param name="year">The year for which to check.</param>
  /// <returns>true if exam is in course in year; otherwise false.</returns>
  public bool IsExamInCourse(int exam, string course, string year)
  {
    Connection.Open();
    const string query = "SELECT * from exam WHERE code=@code and course=@course and year=@year";
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@code", exam);
      command.Parameters.AddWithValue("@course", course);
      command.Parameters.AddWithValue("@year", year);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (reader.Read())
      {
        Connection.Close();
        return true;
      }

      Log.Debug("{exam} {course} {year} doesn't exist", exam, course, year);
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();
    return false;
  }
}