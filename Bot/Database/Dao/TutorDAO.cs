using System.Diagnostics.CodeAnalysis;
using Bot.Database.Records;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class TutorDAO
{
  private readonly MySqlConnection _connection;

  public TutorDAO(MySqlConnection connection)
  {
    _connection = connection;
  }

  /// <summary>
  /// Finds a tutor saved in db.
  /// </summary>
  /// <returns>Tutor if found, otherwise null.</returns>
  public Tutor? FindTutor(int tutorStudentCode)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor WHERE tutor_code=@tutorCode";

    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@tutorCode", tutorStudentCode);
      command.Prepare();
      var reader = command.ExecuteReader();

      if (!reader.Read())
      {
        Log.Debug("No Tutors found in db");
        return null;
      }

      var tutor = new Tutor
      {
        TutorCode = reader.GetInt32("tutor_code"),
        Name = reader.GetString("name"),
        Surname = reader.GetString("surname"),
        Course = reader.GetString("course"),
        Ranking = reader.GetInt32("ranking"),
        OfaAvailable = reader.GetBoolean("OFA_available")
      };


      reader.Close();
      _connection.Close();
      return tutor;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Finds all tutors saved in db.
  /// </summary>
  /// <returns>List of tutors in db.</returns>
  public List<Tutor> FindTutors()
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor";

    var tutors = new List<Tutor>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
      {
        Log.Debug("No Tutors found in db");
      }

      while (reader.Read())
      {
        var tutor = new Tutor
        {
          TutorCode = reader.GetInt32("tutor_code"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available")
        };
        tutors.Add(tutor);
      }

      reader.Close();
      _connection.Close();
      return tutors;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Adds a new possible tutoring to db.
  /// </summary>
  /// <param name="tutorings">tutoring to add</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddTutor(List<TutorToExam> tutorings)
  {
    return AddTutor(tutorings, out _);
  }

  /// <summary>
  /// Adds a new possible tutoring to db.
  /// </summary>
  /// <param name="tutorings">tutoring to add</param>
  /// <param name="errorMessage">error message</param>
  /// <returns>false if anything goes wrong, otherwise true.</returns>
  public bool AddTutor(List<TutorToExam> tutorings, out string errorMessage)
  {
    errorMessage = "";
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query1 = "SELECT * FROM tutor WHERE ranking=@ranking";
    const string query2 = "INSERT INTO tutor (tutor_code,name,surname,course,OFA_available,ranking) " +
                          "VALUES (@tutor,@name,@surname,@course,@OFA_available,@ranking)";
    const string query3 = "INSERT INTO tutor_to_exam (tutor,exam,exam_professor,available_tutorings) " +
                          "VALUES (@tutor,@exam,@professor,@availableTutorings)";
    try
    {
      var command = new MySqlCommand(query1, _connection, transaction);


      foreach (var tutorToExam in tutorings)
      {
        // Clear parameters from last loop
        command.Parameters.Clear();
        // Check if rank is already owned by another tutor
        command.CommandText = "SELECT * FROM tutor WHERE ranking=@ranking";
        command.Parameters.AddWithValue("@ranking", tutorToExam.Ranking);
        command.Prepare();
        var reader = command.ExecuteReader();
        if (reader.Read())
        {
          var code = reader.GetInt32("tutor_code");
          if (code != tutorToExam.TutorCode)
          {
            errorMessage = $"Tried adding tutoring for tutor: {tutorToExam.TutorCode} " +
                           $"with the same rank as tutor: {code}";
            Log.Warning(errorMessage);
            return false;
          }
        }

        reader.Close();
        // Check if tutor already has another rank
        command.CommandText = "SELECT * FROM tutor WHERE tutor_code=@tutor";
        command.Parameters.AddWithValue("@tutor", tutorToExam.TutorCode);
        command.Prepare();
        reader = command.ExecuteReader();

        if (reader.Read())
        {
          var ranking = reader.GetInt32("ranking");
          if (ranking != tutorToExam.Ranking)
          {
            errorMessage = $"Tried adding tutoring for tutor: {tutorToExam.TutorCode} " +
                           $"with rank: {tutorToExam.Ranking} that is different from the already present rank: {ranking}";
            Log.Warning(errorMessage);
            return false;
          }
        }

        reader.Close();
        command.CommandText = query2;
        command.Parameters.AddWithValue("@name", tutorToExam.Name);
        command.Parameters.AddWithValue("@surname", tutorToExam.Surname);
        command.Parameters.AddWithValue("@course", tutorToExam.Course);
        command.Parameters.AddWithValue("@OFA_available", tutorToExam.OfaAvailable);
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
              Log.Debug($"Duplicate key entry while adding tutor: {tutorToExam.TutorCode}");
              break;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              _connection.Close();
              errorMessage =
                $"Adding new tutor: {tutorToExam.TutorCode} with non-existing course: {tutorToExam.Course}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              _connection.Close();
              errorMessage =
                $"Tried adding new tutor: {tutorToExam.TutorCode} with no name or surname";
              Log.Warning(errorMessage);
              return false;
            default:
              Console.WriteLine(e);
              throw;
          }
        }

        command.CommandText = query3;
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@tutor", tutorToExam.TutorCode);
        command.Parameters.AddWithValue("@exam", tutorToExam.ExamCode);
        command.Parameters.AddWithValue("@professor", tutorToExam.Professor);
        command.Parameters.AddWithValue("@availableTutorings", tutorToExam.AvailableTutorings);
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
              _connection.Close();
              errorMessage =
                $"Duplicate key entry while adding exam: {tutorToExam.ExamCode} for tutor: {tutorToExam.TutorCode}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              _connection.Close();
              errorMessage =
                $"Adding tutoring for non-existing exam: {tutorToExam.ExamCode} for tutor: {tutorToExam.TutorCode}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              _connection.Close();
              errorMessage =
                $"Tried adding new tutoring for tutor: {tutorToExam.TutorCode} with missing exam, professor or available tutorings";
              Log.Warning(errorMessage);
              return false;
            default:
              Console.WriteLine(e);
              throw;
          }
        }
      }

      transaction.Commit();
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

  public bool AddTutor(Tutor tutor, out string errorMessage)
  {
    throw new NotImplementedException();
  }

  public int? FindTutorCode(string tutorFullName, int exam)
  {
    var names = tutorFullName.Split(' ');
    if (names.Length < 2)
    {
      Log.Debug("Failed to properly split name: {t}", tutorFullName);
      return null;
    }

    var firstName = names[0];
    var lastName = names[1];
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE name=@firstName AND surname=@lastName AND exam=@exam;";

    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@firstName", firstName);
      command.Parameters.AddWithValue("@lastName", lastName);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();
      if (!reader.Read())
      {
        Log.Debug("No tutor code found for {name} in db", tutorFullName);
        _connection.Close();
        return null;
      }

      var tudorCode = reader.GetInt32("tutor");
      _connection.Close();
      return tudorCode;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  public int? FindTutorCode(string tutorFullName)
  {
    var names = tutorFullName.Split(' ');
    if (names.Length < 2)
    {
      Log.Debug("Failed to properly split name: {t}", tutorFullName);
      return null;
    }

    var firstName = names[0];
    var lastName = names[1];
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE name=@firstName AND surname=@lastName AND OFA_available = 1";

    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@firstName", firstName);
      command.Parameters.AddWithValue("@lastName", lastName);
      command.Prepare();

      var reader = command.ExecuteReader();
      if (!reader.Read())
      {
        Log.Debug("No tutor code found for {name} in db", tutorFullName);
        _connection.Close();
        return null;
      }

      var tudorCode = reader.GetInt32("tutor");
      _connection.Close();
      return tudorCode;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  public TutorToExam? FindTutoring(int tutor, int exam)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE tutor=@tutor AND exam=@exam;";

    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();
      if (!reader.Read())
      {
        Log.Debug("No tutoring found with code {0} for exam {1} in db", tutor, exam);
        _connection.Close();
        return null;
      }

      var tutorObj = new TutorToExam
      {
        TutorCode = reader.GetInt32("tutor"),
        Name = reader.GetString("name"),
        Surname = reader.GetString("surname"),
        ExamCode = reader.GetInt32("exam"),
        Professor = reader.GetString("exam_professor"),
        Course = reader.GetString("course"),
        Ranking = reader.GetInt32("ranking"),
        OfaAvailable = reader.GetBoolean("OFA_available"),
        LastReservation = reader.GetDateTime("last_reservation"),
        AvailableTutorings = reader.GetInt32("available_tutorings")
      };
      _connection.Close();
      return tutorObj;
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Finds all possible tutorings from a tutor.
  /// </summary>
  /// <param name="tutorCode">Student code of tutor</param>
  /// <returns>List of tutorings from a tutor.</returns>
  public List<TutorToExam> FindTutorings(int tutorCode)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE tutor=@code;";

    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@code", tutorCode);
      command.Prepare();

      var reader = command.ExecuteReader();
      if (!reader.HasRows)
      {
        Log.Debug("No tutorings found for tutor {name} in db", tutorCode);
      }
      else
      {
        while (reader.Read())
        {
          var tutoring = new TutorToExam
          {
            TutorCode = reader.GetInt32("tutor"),
            Name = reader.GetString("name"),
            Surname = reader.GetString("surname"),
            ExamCode = reader.GetInt32("exam"),
            Professor = reader.GetString("exam_professor"),
            Course = reader.GetString("course"),
            Ranking = reader.GetInt32("ranking"),
            OfaAvailable = reader.GetBoolean("OFA_available"),
            LastReservation = reader.GetDateTime("last_reservation"),
            AvailableTutorings = reader.GetInt32("available_tutorings")
          };
          tutors.Add(tutoring);
        }
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds all possible tutorings.
  /// </summary>
  /// <returns>List of possible tutorings.</returns>
  public List<TutorToExam> FindTutorings()
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor join tutor_to_exam";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No tutors found");

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          ExamCode = reader.GetInt32("exam"),
          Professor = reader.GetString("exam_professor"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available"),
          LastReservation = reader.GetDateTime("last_reservation"),
          AvailableTutorings = reader.GetInt32("available_tutorings")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds tutoring, for a specific exam.
  /// </summary>
  /// <param name="exam">Exam for which to find tutoring.</param>
  /// <returns>List of tutorings for exam.</returns>
  public List<TutorToExam> FindTutorings(string exam)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                         "WHERE exam=@exam";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No tutors found for {exam} in db", exam);

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          ExamCode = reader.GetInt32("exam"),
          Professor = reader.GetString("exam_professor"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available"),
          LastReservation = reader.GetDateTime("last_reservation"),
          AvailableTutorings = reader.GetInt32("available_tutorings")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds tutorings from ActiveTutoring table.
  /// </summary>
  /// <param name="active">True to find active tutorings, false to find ended tutorings.</param>
  /// <returns>List of tutorings from ActiveTutoring table.</returns>
  public List<ActiveTutoring> FindActiveTutorings(bool active)
  {
    _connection.Open();
    var query = active
      ? "SELECT * FROM active_tutoring join tutor on tutor=tutor_code " +
        "WHERE end_date IS NULL"
      : "SELECT * FROM active_tutoring join tutor on tutor=tutor_code " +
        "WHERE end_date IS NOT NULL";

    var tutorings = new List<ActiveTutoring>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug($"No active={active} tutorings found");

      while (reader.Read())
      {
        var tutoring = new ActiveTutoring
        {
          Id = reader.GetInt32("ID"),
          TutorCode = reader.GetInt32("tutor"),
          TutorName = reader.GetString("name"),
          TutorSurname = reader.GetString("surname"),
          StudentCode = reader.GetInt32("student"),
          IsOFA = reader.GetBoolean("is_OFA"),
          StartDate = reader.GetDateTime("start_date"),
        };
        if (!tutoring.IsOFA)
          tutoring.ExamCode = reader.GetInt32("exam");
        if (!active)
        {
          tutoring.EndDate = reader.GetDateTime("end_date");
          tutoring.Duration = reader.GetInt32("duration");
        }

        tutorings.Add(tutoring);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutorings;
  }

  /// <summary>
  /// Finds tutors, that are locked, for a specific exam.
  /// </summary>
  /// <param name="exam">Exam for which to find tutors.</param>
  /// <param name="hoursSinceLock">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
  /// <returns>List of locked tutors for exam.</returns>
  public List<TutorToExam> FindLockedTutors(string exam, int hoursSinceLock)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on name=tutor " +
                         "WHERE exam=@exam AND lock_timestamp >= NOW() - INTERVAL @hours HOUR";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@exam", exam);
      command.Parameters.AddWithValue("@hours", hoursSinceLock);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No unlocked tutors found for {exam} in db", exam);

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          ExamCode = reader.GetInt32("exam"),
          Professor = reader.GetString("exam_professor"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available"),
          LastReservation = reader.GetDateTime("last_reservation"),
          AvailableTutorings = reader.GetInt32("available_tutorings")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds tutors, that are available, for a specific exam.
  /// A tutor is available if it hasn't had a reservation in the last 24 hours and if it has
  /// </summary>
  /// <param name="exam">The code of the exam for which to find tutors.</param>
  /// <param name="lockHours">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
  /// <returns>List of non locked tutors for exam.</returns>
  public List<TutorToExam> FindAvailableTutors(int exam, int lockHours)
  {
    _connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code=tutor " +
                         "WHERE exam=@exam AND last_reservation <= NOW() - INTERVAL @hours HOUR " +
                         "AND available_reservations > 0 ORDER BY ranking ASC";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@exam", exam);
      command.Parameters.AddWithValue("@hours", lockHours);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No unlocked tutors found for {exam} in db", exam);

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          ExamCode = reader.GetInt32("exam"),
          Professor = reader.GetString("exam_professor"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available"),
          LastReservation = reader.GetDateTime("last_reservation"),
          AvailableTutorings = reader.GetInt32("available_tutorings")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();

    return tutors;
  }


  /// <summary>
  /// Checks if if a tutor teaches a given exam.
  /// </summary>
  /// <param name="tutor">The tutor for which to check.</param>
  /// <param name="exam">The exam to check.</param>
  /// <returns>true if the tutor teaches the exam; otherwise false.</returns>
  public bool IsTutorForExam(int tutor, int exam)
  {
    _connection.Open();
    const string query =
      "SELECT * from tutor join tutor_to_exam on tutor_code=tutor WHERE tutor_code=@tutor AND exam=@exam;";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (reader.HasRows)
      {
        _connection.Close();
        return true;
      }

      Log.Debug("Tutor {tutor} not found found for exam {exam} in db", tutor, exam);
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
  /// Updates last reservation time stamp in tutor_to_exam table, updated user lock time stamp
  /// <br/> and inserts reservation into reservation table.
  /// </summary>
  /// <param name="tutor">Student code of Tutor that needs to be reserved.</param>
  /// <param name="exam">Code of exam that needs to be reserved.</param>
  /// <param name="user">Telegram ID of user that reserved the tutor.</param>
  /// <param name="studentCode">StudentCode of the user that made the reservation</param>
  public void ReserveTutor(int tutor, int exam, long user, int studentCode)
  {
    //TODO: trasformare in una transazione con lock dell'utente e aggiornare doc
    _connection.Open();
    var transaction = _connection.BeginTransaction();

    try
    {
      const string query =
        "UPDATE tutor_to_exam SET last_reservation = NOW()" +
        "WHERE tutor=@tutor AND exam=@exam";
      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();
      command.ExecuteNonQuery();

      command.Parameters.Clear();
      command.CommandText = "UPDATE telegram_user SET lock_timestamp = NOW() "
                            + "WHERE userID = @userID";
      command.Parameters.AddWithValue("@userID", user);
      command.Prepare();
      command.ExecuteNonQuery();

      command.Parameters.Clear();
      command.CommandText = "INSERT INTO reservation (tutor,exam,student) VALUES (@tutor,@exam,@student)";
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Parameters.AddWithValue("@student", studentCode);
      command.Prepare();
      command.ExecuteNonQuery();

      transaction.Commit();
      Log.Debug("Tutor {tutor} was reserved", tutor);
    }
    catch (Exception e)
    {
      Log.Error("Exception while user {0} with student code {1} was reserving tutor {2} for exam {3}"
        , user, studentCode, tutor, exam);
      transaction.Rollback();
      _connection.Close();
      throw;
    }

    _connection.Close();
  }

  /// <summary>
  /// Updates user lock time stamp and inserts OFA reservation into reservation table.
  /// </summary>
  /// <param name="tutor">Student code of Tutor that needs to be reserved.</param>
  /// <param name="user">Telegram ID of user that reserved the tutor.</param>
  /// <param name="studentCode">StudentCode of the user that made the reservation</param>
  public void ReserveOFATutor(int tutor, long user, int studentCode)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();

    try
    {
      const string query = "UPDATE telegram_user SET lock_timestamp = NOW() "
                           + "WHERE userID = @userID";

      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@userID", user);
      command.Prepare();
      command.ExecuteNonQuery();

      command.Parameters.Clear();
      command.CommandText = "INSERT INTO reservation (tutor,student,is_OFA) VALUES (@tutor,@student,1)";
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@student", studentCode);
      command.Prepare();
      command.ExecuteNonQuery();

      transaction.Commit();
      Log.Debug("Tutor {tutor} was reserved", tutor);
    }
    catch (Exception e)
    {
      Log.Error("Exception while user {0} with student code {1} was reserving tutor {2} for OFA"
        , user, studentCode, tutor);
      transaction.Rollback();
      _connection.Close();
      throw;
    }

    _connection.Close();
  }

  /// <summary>
  /// Updates the lock_timestamp of the given tutor for the given exam to the current time.<br/>
  /// This Method is only to be called by the web api.
  /// </summary>
  /// <param name="tutor">Name of Tutor that needs to be updated.</param>
  /// <param name="exam">Name of exam that needs to be locked.</param>
  public void LockTutor(string tutor, string exam)
  {
    _connection.Open();
    const string query =
      "UPDATE tutor_to_exam SET lock_timestamp = NOW(), locked_by=DEFAULT WHERE tutor=@tutor AND exam=@exam";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      command.ExecuteNonQuery();
      Log.Debug("Tutor {tutor} was locked", tutor);
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
  }

  /// <summary>
  /// Updates the lock_timestamp of the given tutor for the given exam to the 0000-00-00 00:00:00 DEFAULT timestamp.
  /// </summary>
  /// <param name="tutor">Name of Tutor that needs to be updated.</param>
  /// <param name="exam">Name of exam that needs to be unlocked.</param>
  public void UnlockTutor(string tutor, string exam)
  {
    _connection.Open();
    // start local transaction
    var transaction = _connection.BeginTransaction();
    const string query1 = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                          "WHERE userID IN " +
                          "(select locked_by from tutor_to_exam where tutor=@tutor AND exam=@exam)";

    const string query2 = "UPDATE tutor_to_exam SET lock_timestamp = DEFAULT ,locked_by = DEFAULT " +
                          "WHERE tutor=@tutor AND exam=@exam;";
    try
    {
      var command = new MySqlCommand(query1, _connection, transaction);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = query2;
      command.Parameters.Clear();
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();
      command.ExecuteNonQuery();

      transaction.Commit();
      Log.Debug("Tutor {tutor} and gatekeeper user were unlocked", tutor);
    }
    catch (Exception)
    {
      // Attempt to roll back the transaction.
      try
      {
        transaction.Rollback();
      }
      catch (Exception ex2)
      {
        // This catch block will handle any errors that may have occurred
        // on the server that would cause the rollback to fail, such as
        // a closed connection.
        Log.Error("Rollback Exception Type: {0}", ex2.GetType());
        Log.Error("  Message: {0}", ex2.Message);
      }

      _connection.Close();
      throw;
    }

    _connection.Close();
  }

  /// <summary>
  /// Deletes a tutor from db.
  /// </summary>
  /// <param name="tutor">Name of Tutor that needs to be deleted.</param>
  public void DeleteTutor(string tutor)
  {
    _connection.Open();
    const string query = "DELETE FROM tutor WHERE (`name` = @tutor)";
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Prepare();

      command.ExecuteNonQuery();
      Log.Debug("Tutor {tutor} was deleted", tutor);
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
  }


  /// <summary>
  /// Activates a tutoring from a given reservation.
  /// </summary>
  /// <param name="reservationId">Id of the given reservation</param>
  public void ActivateTutoring(int reservationId)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query =
      "UPDATE tutor_to_exam as t SET available_tutorings = available_tutorings - 1, last_reservation = DEFAULT " +
      "WHERE EXISTS (select * FROM reservation as res WHERE ID=@reservationId AND " +
      "res.exam = t.exam AND res.tutor = t.tutor);";
    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@reservationId", reservationId);
      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = "UPDATE reservation SET is_processed = 1 " +
                            "WHERE ID=@reservationId;";
      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                            "WHERE student_code IN (select student FROM reservation WHERE ID=@reservationId);";
      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = "INSERT INTO active_tutoring (tutor, exam, student, is_OFA) " +
                            "SELECT tutor, exam, student, is_OFA " +
                            "FROM reservation " +
                            "WHERE ID=@reservationId;";
      command.Prepare();
      command.ExecuteNonQuery();
      transaction.Commit();
    }
    catch (Exception e)
    {
      transaction.Rollback();
      _connection.Close();
      throw;
    }

    _connection.Close();
  }

  /// <summary>
  /// Activates a tutoring from a given tutor, student and exam.
  /// </summary>
  public void ActivateTutoring(int tutor, int student, int exam)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query =
      "UPDATE tutor_to_exam as t SET available_tutorings = available_tutorings - 1, last_reservation = DEFAULT " +
      "WHERE exam = @examCode AND tutor = @tutorCode";
    
    // Find out if the same tutoring is already active
    const string query2 = "SELECT * FROM active_tutoring WHERE tutor=@tutorCode " +
                          "AND student=@studentCode AND duration IS NULL";
    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@examCode", exam);
      command.Parameters.AddWithValue("@tutorCode", tutor);
      command.Prepare();
      command.ExecuteNonQuery();
  
      command.CommandText = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                            "WHERE student_code = @studentCode";
      
      command.CommandText = query2;
      command.Parameters.AddWithValue("@studentCode", student);
      command.Prepare();
      var result = command.ExecuteScalar();
      if (result != null)
      {
        Log.Error($"Tried activating an already active tutoring for tutor: {tutor} and student: {student}");
        _connection.Close();
        return;
      }
      
      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = "INSERT INTO active_tutoring (tutor, exam, student) " +
                            "VALUES (@tutorCode, @examCode, @studentCode)";
      command.Prepare();
      command.ExecuteNonQuery();
      transaction.Commit();
    }
    catch (Exception e)
    {
      switch (e)
      {
        case MySqlException { Number: 1062 }:
          // duplicate key entry
          Log.Debug($"Duplicate key entry while activating tutoring for student: {student}");
          break;
        default:
          Console.WriteLine(e);
          throw;
      }
      _connection.Close();
    }

    _connection.Close();
  }
  
  /// <summary>
  /// Activates an OFA tutoring from a tutor for a given student.
  /// </summary>
  public void ActivateTutoring(int tutor, int student)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    // Find out if tutor is available for OFA
    const string query =
      "SELECT * FROM tutor WHERE tutor_code = @tutorCode AND OFA_available = 1 ";

    // Find out if the same tutoring is already active
    const string query2 = "SELECT * FROM active_tutoring WHERE tutor=@tutorCode " +
                          "AND student=@studentCode AND duration IS NULL";
    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@tutorCode", tutor);
      command.Prepare();
      var result = command.ExecuteScalar();
      if (result == null)
      {
        Log.Error($"Tried activating an OFA tutoring for tutor:{tutor} who isn't OFA available");
        _connection.Close();
        return;
      }

      command.CommandText = query2;
      command.Parameters.AddWithValue("@studentCode", student);
      command.Prepare();
      result = command.ExecuteScalar();
      if (result != null)
      {
        Log.Error($"Tried activating an already active tutoring for tutor: {tutor} and student: {student}");
        _connection.Close();
        return;
      }
      
      command.CommandText = "UPDATE telegram_user SET lock_timestamp = DEFAULT " +
                            "WHERE student_code = @studentCode";

      command.Prepare();
      command.ExecuteNonQuery();

      command.CommandText = "INSERT INTO active_tutoring (tutor, student, is_OFA) " +
                            "VALUES (@tutorCode, @studentCode, 1)";
      command.Prepare();
      command.ExecuteNonQuery();
      transaction.Commit();
    }
    catch (Exception e)
    {
      switch (e)
      {
        case MySqlException { Number: 1062 }:
          // duplicate key entry
          Log.Debug($"Duplicate key entry while activating tutoring for student: {student}");
          break;
        default:
          Console.WriteLine(e);
          throw;
      }
      _connection.Close();
    }

    _connection.Close();
  }

  /// <summary>
  /// Ends a tutoring by adding an end date to an active tutoring,
  /// and if it wasn't for OFA it updates the number of available tutorings for the relative exam.
  /// </summary>
  /// <param name="id">Id of the tutoring to End</param>
  /// <param name="duration">Duration in hours of the tutoring.</param>
  /// <returns>false if no rows where affected, otherwise true</returns>
  public bool EndTutoring(int id, int duration)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query =
      "UPDATE active_tutoring SET end_date = CURRENT_TIMESTAMP, duration = @duration WHERE ID = @id AND student = @studentCode";
    try
    {
      var command = new MySqlCommand(query, _connection, transaction);
      command.Parameters.AddWithValue("@id", id);
      command.Parameters.AddWithValue("@duration", duration);
      command.Prepare();
      var affectedRows = command.ExecuteNonQuery();
      if (affectedRows == 0)
      {
        transaction.Commit();
        _connection.Close();
        Log.Debug($"Tried ending a tutoring with id: {id} that didn't exist");
        return false;
      }

      command.CommandText = "SELECT * FROM active_tutoring WHERE ID=@id";
      command.Parameters.Clear();
      command.Parameters.AddWithValue("@id", id);
      command.Prepare();

      var reader = command.ExecuteReader();
      reader.Read();

      var tutor = reader.GetInt32("tutor");
      var studentCode = reader.GetInt32("student");
      var isOfa = reader.GetBoolean("is_OFA");

      if (!isOfa)
      {
        var exam = reader.GetInt32("exam");
        ;
        reader.Close();
        command.CommandText = "UPDATE tutor_to_exam SET available_tutorings = available_tutorings + 1 " +
                              "WHERE exam=@exam AND tutor=@tutor";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@exam", exam);
        command.Parameters.AddWithValue("@tutor", tutor);
        command.Prepare();
        command.ExecuteNonQuery();
      }

      transaction.Commit();
      Log.Debug("Tutoring from tutor: {tutor} to student: {studentCode} was ended", tutor, studentCode);
    }
    catch (Exception)
    {
      transaction.Rollback();
      _connection.Close();
      throw;
    }

    _connection.Close();
    return true;
  }

  /// <summary>
  /// Ends a tutoring by adding an end date to an active tutoring,
  /// and if it wasn't for OFA it updates the number of available tutorings for the relative exam.
  /// </summary>
  /// <param name="id">Id of the tutoring to End</param>
  /// <param name="duration">Duration in hours of the tutoring.</param>
  /// <returns>false if no rows where affected, otherwise true</returns>
  public bool EndTutorings(List<TutoringToDuration> durations)
  {
    _connection.Open();
    var transaction = _connection.BeginTransaction();
    const string query =
      "UPDATE active_tutoring SET end_date = CURRENT_TIMESTAMP, duration = @duration WHERE ID = @id";
    try
    {
      foreach (var (id, duration) in durations)
      {
        var command = new MySqlCommand(query, _connection, transaction);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@duration", duration);
        command.Prepare();
        var affectedRows = command.ExecuteNonQuery();
        if (affectedRows == 0)
        {
          transaction.Commit();
          _connection.Close();
          Log.Debug($"Tried ending a tutoring with id: {id} that didn't exist");
          return false;
        }

        command.CommandText = "SELECT * FROM active_tutoring WHERE ID=@id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@id", id);
        command.Prepare();

        var reader = command.ExecuteReader();
        reader.Read();

        var tutor = reader.GetInt32("tutor");
        var studentCode = reader.GetInt32("student");
        var isOfa = reader.GetBoolean("is_OFA");

        if (isOfa)
        {
          reader.Close();
          continue;
        }

        var exam = reader.GetInt32("exam");
        ;
        reader.Close();
        command.CommandText = "UPDATE tutor_to_exam SET available_tutorings = available_tutorings + 1 " +
                              "WHERE exam=@exam AND tutor=@tutor";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@exam", exam);
        command.Parameters.AddWithValue("@tutor", tutor);
        command.Prepare();
        command.ExecuteNonQuery();
      }

      transaction.Commit();
      Log.Debug($"Given tutorings were ended.");
    }
    catch (Exception)
    {
      transaction.Rollback();
      _connection.Close();
      throw;
    }

    _connection.Close();
    return true;
  }

  public List<TutorToExam> FindAvailableOFATutors(int lockHours)
  {
    //TODO: check if tutor is already in an active tutoring
    _connection.Open();
    const string query = "SELECT * FROM tutor " +
                         "WHERE OFA_available = 1";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, _connection);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No unlocked tutors found for OFA in db");

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor_code"),
          Name = reader.GetString("name"),
          Surname = reader.GetString("surname"),
          Course = reader.GetString("course"),
          Ranking = reader.GetInt32("ranking"),
          OfaAvailable = reader.GetBoolean("OFA_available")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      _connection.Close();
      throw;
    }

    _connection.Close();
    return tutors;
  }
}