using Bot.Database.Records;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class CourseDAO
{
  private readonly MySqlConnection _connection;

  public CourseDAO(MySqlConnection connection)
  {
    _connection = connection;
  }


  /// <summary>
  /// Deletes all courses and tutorings from the db
  /// </summary>
  public void DeleteCourses()
  {
    _connection.Open();
    const string query = "DELETE FROM course ";

    try
    {
      var command = new MySqlCommand(query, _connection);
      command.ExecuteNonQuery();
      Log.Debug("All courses where deleted from db");
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Adds new courses to db.
  /// </summary>
  /// <param name="courses">courses to add</param>
  /// <param name="errorMessage">error message</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddCourse(List<Course> courses, out string errorMessage)
  {
    errorMessage = "";
    _connection.Open();
    const string query = "INSERT INTO course (name,school) " +
                         "VALUES (@name,@school)";
    try
    {
      var command = new MySqlCommand(query, _connection);


      foreach (var course in courses)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@name", course.Name);
        command.Parameters.AddWithValue("@school", course.School);
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
              errorMessage = $"Duplicate entry for course: {course.Name}";
              Log.Debug(errorMessage);
              break;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              _connection.Close();
              errorMessage =
                $"Adding new course: {course.Name} with non-existing school: {course.School}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              _connection.Close();
              errorMessage =
                $"Tried adding new exam: {course.Name} with an empty school value";
              Log.Warning(errorMessage);
              return false;
            default:
              throw;
          }
        }
      }

      _connection.Close();
      return true;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Adds new course to db.
  /// </summary>
  /// <param name="course">course to add</param>
  /// <param name="errorMessage">error message</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddCourse(Course course, out string errorMessage)
  {
    var courses = new List<Course> { course };
    return AddCourse(courses, out errorMessage);
  }

  /// <summary>
  /// Finds courses in db.
  /// </summary>
  /// <returns>List of courses in given school.</returns>
  public List<string> FindCourses()
  {
    _connection.Open();
    const string query = "SELECT * from course";
    var courses = new List<string>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No courses found in Db");

      while (reader.Read())
        courses.Add(reader.GetString("name"));
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return courses;
  }

  /// <summary>
  /// Finds courses in given school.
  /// </summary>
  /// <param name="school">The school in which to search.</param>
  /// <returns>List of courses in given school.</returns>
  public List<string> FindCourses(string school)
  {
    _connection.Open();
    const string query = "SELECT * from course WHERE school=@school";
    var courses = new List<string>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@school", school);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No courses found for school {school}", school);

      while (reader.Read())
        courses.Add(reader.GetString("name"));
    }
    catch (Exception e)
    {
      _connection.Close();
      Console.WriteLine(e);
    }

    _connection.Close();
    return courses;
  }

  /// <summary>
  /// Determines whether a School contains a specified Course 
  /// </summary>
  /// <param name="course">The Course to check</param> 
  /// <param name="school">The School in which to check</param> 
  /// <returns>true if the specified School contains the given Course; otherwise, false</returns>
  public bool IsCourseInSchool(string course, string school)
  {
    _connection.Open();
    const string query = "SELECT * from course WHERE name=@name";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@name", course);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (reader.Read())
      {
        var foundSchool = reader.GetString("school");
        if (foundSchool == school)
        {
          _connection.Close();
          return true;
        }

        Log.Debug("Course {course} is of School {foundSchool} not {school}."
          , course, foundSchool, school);
      }
      else
      {
        Log.Debug("No courses found for school {school}", school);
      }
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
  /// Finds available years for a course. 
  /// </summary>
  /// <param name="course">The Course for which to check.</param> 
  /// <returns>Available years for course.</returns>
  public List<string> AvailableYearsInCourse(string course)
  {
    _connection.Open();
    const string query = "SELECT DISTINCT year from exam WHERE course=@course";
    var years = new List<string>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@course", course);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
      {
        Log.Error("No years found in DB");
      }

      while (reader.Read())
      {
        years.Add(reader.GetString("year"));
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return years;
  }
}