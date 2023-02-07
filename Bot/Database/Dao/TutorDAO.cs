using Bot.Database.Records;
using MySql.Data.MySqlClient;
using Serilog;

namespace Bot.Database.Dao;

public class TutorDAO : DAO
{
  public TutorDAO(MySqlConnection connection) : base(connection)
  {
  }

  /// <summary>
  /// Changes the contract state of a given tutor.
  /// </summary>
  /// <param name="tutorCode">Student code of the tutor whose state must be changed.</param>
  /// <param name="newState">New contract state for tutor.</param>
  /// <returns>true if contract state change was a success, otherwise false.</returns>
  public bool ChangeContractState(int tutorCode, int newState)
  {
    Connection.Open();
    const string query = "UPDATE tutor SET contract_state = @state " +
                         "WHERE tutor_code=@tutorCode";

    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@tutorCode", tutorCode);
      command.Parameters.AddWithValue("@state", newState);
      command.Prepare();
      var result = command.ExecuteNonQuery();

      if (result != 0)
      {
        Connection.Close();
        return true;
      }

      Log.Debug($"Tried changing contract of non existing tutor: {tutorCode}");
      Connection.Close();
      return false;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Finds a tutor saved in db.
  /// </summary>
  /// <returns>Tutor if found, otherwise null.</returns>
  public Tutor? FindTutor(int tutorStudentCode)
  {
    Connection.Open();
    const string query = "SELECT * FROM tutor WHERE tutor_code=@tutorCode";

    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@tutorCode", tutorStudentCode);
      command.Prepare();
      var reader = command.ExecuteReader();

      if (!reader.Read())
      {
        Log.Debug("No Tutors found in db");
        Connection.Close();
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
      Connection.Close();
      return tutor;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Finds all tutors saved in db.
  /// </summary>
  /// <returns>List of tutors in db.</returns>
  public List<Tutor> FindTutors()
  {
    Connection.Open();
    const string query = "SELECT * FROM tutor";

    var tutors = new List<Tutor>();
    try
    {
      var command = new MySqlCommand(query, Connection);
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
          OfaAvailable = reader.GetBoolean("OFA_available"),
          ContractState = reader.GetInt32("contract_state")
        };
        tutors.Add(tutor);
      }

      reader.Close();
      Connection.Close();
      return tutors;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      Connection.Close();
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
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    const string query1 = "SELECT * FROM tutor WHERE ranking=@ranking";
    const string query2 = "INSERT INTO tutor (tutor_code,name,surname,course,OFA_available,ranking) " +
                          "VALUES (@tutor,@name,@surname,@course,@OFA_available,@ranking)";
    const string query3 = "INSERT INTO tutor_to_exam (tutor,exam,exam_professor,available_tutorings) " +
                          "VALUES (@tutor,@exam,@professor,@availableTutorings)";
    try
    {
      var command = new MySqlCommand(query1, Connection, transaction);


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
            Connection.Close();
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
            Connection.Close();
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
          Connection.Close();
          switch (e)
          {
            case MySqlException { Number: 1062 }:
              // duplicate key entry
              Log.Debug($"Duplicate key entry while adding tutor: {tutorToExam.TutorCode}");
              break;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              errorMessage =
                $"Adding new tutor: {tutorToExam.TutorCode} with non-existing course: {tutorToExam.Course}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              errorMessage =
                $"Tried adding new tutor: {tutorToExam.TutorCode} with no name or surname";
              Log.Warning(errorMessage);
              return false;
            default:
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
          Connection.Close();
          switch (e)
          {
            case MySqlException { Number: 1062 }:
              // duplicate key entry
              errorMessage =
                $"Duplicate key entry while adding exam: {tutorToExam.ExamCode} for tutor: {tutorToExam.TutorCode}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1452 }:
              // foreign key fail
              errorMessage =
                $"Adding tutoring for non-existing exam: {tutorToExam.ExamCode} for tutor: {tutorToExam.TutorCode}";
              Log.Warning(errorMessage);
              return false;
            case MySqlException { Number: 1048 }:
              // null value
              errorMessage =
                $"Tried adding new tutoring for tutor: {tutorToExam.TutorCode} with missing exam, professor or available tutorings";
              Log.Warning(errorMessage);
              return false;
            default:
              throw;
          }
        }
      }

      transaction.Commit();
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
  /// Tries to delete a list of tutorings.
  /// </summary>
  /// <param name="tutorToExamList">List of tutorings to delete.</param>
  /// <param name="errorMessage">Deletion error.</param>
  /// <returns>true if deletion worked for all tutorings, otherwise false.</returns>
  public bool DeleteTutorings(List<TutorCodeToExamCode> tutorToExamList, out string errorMessage)
  {
    Connection.Open();
    const string query = "DELETE FROM tutor_to_exam " +
                         "WHERE tutor=@tutor AND exam=@exam;";

    var transaction = Connection.BeginTransaction();
    try
    {
      var command = new MySqlCommand(query, Connection, transaction);
      foreach (var tutorToExam in tutorToExamList)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@tutor", tutorToExam.TutorCode);
        command.Parameters.AddWithValue("@exam", tutorToExam.ExamCode);
        command.Prepare();
        var result = command.ExecuteNonQuery();
        if (result != 0)
          continue;
        errorMessage = $"No tutoring found in db with code {tutorToExam.TutorCode} for exam {tutorToExam.ExamCode}";
        Log.Error(errorMessage);
        Connection.Close();
        return false;
      }

      errorMessage = "";
      transaction.Commit();
      Connection.Close();
      return true;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }

  /// <summary>
  /// Deletes all tutors and tutorings from the db
  /// </summary>
  public void DeleteTutors()
  {
    Connection.Open();
    const string query = "DELETE FROM tutor ";

    try
    {
      var command = new MySqlCommand(query, Connection);
      command.ExecuteNonQuery();
      Log.Debug("All tutors where deleted from db");
      Connection.Close();
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }


  public TutorToExam? FindTutoring(int tutor, int exam)
  {
    Connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE tutor=@tutor AND exam=@exam;";

    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();
      if (!reader.Read())
      {
        Log.Debug("No tutoring found with code {0} for exam {1} in db", tutor, exam);
        Connection.Close();
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
      Connection.Close();
      return tutorObj;
    }
    catch (Exception)
    {
      Connection.Close();
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
    Connection.Open();
    const string query = "SELECT * FROM tutor_to_exam join tutor on tutor_code = tutor " +
                         "WHERE tutor=@code;";

    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds all possible tutorings.
  /// </summary>
  /// <returns>List of possible tutorings.</returns>
  public List<TutorToExam> FindTutorings()
  {
    Connection.Open();
    const string query = "SELECT  *, tutor.name as tutorName,exam.name as examName " +
                         "FROM tutor join tutor_to_exam on tutor_code = tutor " +
                         "join exam on exam=code";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
      var reader = command.ExecuteReader();

      if (!reader.HasRows)
        Log.Debug("No tutors found");

      while (reader.Read())
      {
        var tutor = new TutorToExam
        {
          TutorCode = reader.GetInt32("tutor"),
          Name = reader.GetString("tutorName"),
          Surname = reader.GetString("surname"),
          ExamCode = reader.GetInt32("exam"),
          ExamName = reader.GetString("examName"),
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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return tutors;
  }

  /// <summary>
  /// Finds tutorings from ActiveTutoring table.
  /// </summary>
  /// <param name="active">True to find active tutorings, false to find ended tutorings.</param>
  /// <returns>List of tutorings from ActiveTutoring table.</returns>
  public List<ActiveTutoring> FindActiveTutorings(bool active)
  {
    Connection.Open();
    var query = active
      ? "SELECT * FROM active_tutoring join tutor on tutor=tutor_code " +
        "WHERE end_date IS NULL"
      : "SELECT * FROM active_tutoring join tutor on tutor=tutor_code " +
        "WHERE end_date IS NOT NULL";

    var tutorings = new List<ActiveTutoring>();
    try
    {
      var command = new MySqlCommand(query, Connection);
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
          StartDate = reader.GetDateTime("start_date")
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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return tutorings;
  }

  /// <summary>
  /// Finds tutors, that are available, for a specific exam.
  /// A tutor is available if it hasn't had a reservation in the last 24 hours and if it has
  /// enough available tutorings.
  /// </summary>
  /// <param name="exam">The code of the exam for which to find tutors.</param>
  /// <param name="lockHours">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
  /// <returns>List of non locked tutors for exam.</returns>
  public List<TutorToExam> FindAvailableTutors(int exam, int lockHours)
  {
    Connection.Open();
    const string query = "SELECT * FROM tutor_to_exam as e " +
                         "JOIN tutor as t on t.tutor_code=e.tutor " +
                         "JOIN course as c on c.name=t.course " +
                         "WHERE exam=@exam AND last_reservation <= NOW() - INTERVAL @hours HOUR " +
                         "AND available_tutorings > 0 ORDER BY ranking ASC";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
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
          AvailableTutorings = reader.GetInt32("available_tutorings"),
          School = reader.GetString("school")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();

    return tutors;
  }

  public IEnumerable<TutorToExam> FindAdditionalAvailableTutors(int exam, string examName, int lockHours)
  {
    Connection.Open();
    const string query = "SELECT * FROM tutor_to_exam as te " +
                         "JOIN tutor as t on t.tutor_code=te.tutor " +
                         "JOIN course as c on c.name=t.course " +
                         "JOIN exam as e on e.code=te.exam " +
                         "WHERE exam!=@exam AND e.name = @examName AND last_reservation <= (NOW() - INTERVAL @hours HOUR) " +
                         "AND available_tutorings > 0 ORDER BY ranking ASC";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@exam", exam);
      command.Parameters.AddWithValue("@examName", examName);
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
          AvailableTutorings = reader.GetInt32("available_tutorings"),
          School = reader.GetString("school")
        };
        tutors.Add(tutor);
      }
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();

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
    Connection.Open();
    const string query =
      "SELECT * from tutor join tutor_to_exam on tutor_code=tutor WHERE tutor_code=@tutor AND exam=@exam;";
    try
    {
      var command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@tutor", tutor);
      command.Parameters.AddWithValue("@exam", exam);
      command.Prepare();

      var reader = command.ExecuteReader();

      if (reader.HasRows)
      {
        Connection.Close();
        return true;
      }

      Log.Debug("Tutor {tutor} not found found for exam {exam} in db", tutor, exam);
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }

    Connection.Close();
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
    Connection.Open();
    var transaction = Connection.BeginTransaction();

    try
    {
      const string query =
        "UPDATE tutor_to_exam SET last_reservation = NOW()" +
        "WHERE tutor=@tutor AND exam=@exam";
      var command = new MySqlCommand(query, Connection, transaction);
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
    catch (Exception)
    {
      Log.Error("Exception while user {0} with student code {1} was reserving tutor {2} for exam {3}"
        , user, studentCode, tutor, exam);
      transaction.Rollback();
      Connection.Close();
      throw;
    }

    Connection.Close();
  }

  /// <summary>
  /// Updates user lock time stamp and inserts OFA reservation into reservation table.
  /// </summary>
  /// <param name="tutor">Student code of Tutor that needs to be reserved.</param>
  /// <param name="user">Telegram ID of user that reserved the tutor.</param>
  /// <param name="studentCode">StudentCode of the user that made the reservation</param>
  public void ReserveOFATutor(int tutor, long user, int studentCode)
  {
    Connection.Open();
    var transaction = Connection.BeginTransaction();

    try
    {
      const string query = "UPDATE telegram_user SET lock_timestamp = NOW() "
                           + "WHERE userID = @userID";

      var command = new MySqlCommand(query, Connection, transaction);
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
    catch (Exception)
    {
      Log.Error("Exception while user {0} with student code {1} was reserving tutor {2} for OFA"
        , user, studentCode, tutor);
      transaction.Rollback();
      Connection.Close();
      throw;
    }

    Connection.Close();
  }

  /// <summary>
  /// Activates a tutoring from a given reservation.
  /// </summary>
  /// <param name="reservationId">Id of the given reservation</param>
  public void ActivateTutoring(int reservationId)
  {
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    const string query =
      "UPDATE tutor_to_exam as t SET available_tutorings = available_tutorings - 1, last_reservation = DEFAULT " +
      "WHERE EXISTS (select * FROM reservation as res WHERE ID=@reservationId AND " +
      "res.exam = t.exam AND res.tutor = t.tutor);";
    try
    {
      var command = new MySqlCommand(query, Connection, transaction);
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
    catch (Exception)
    {
      transaction.Rollback();
      Connection.Close();
      throw;
    }

    Connection.Close();
  }

  /// <summary>
  /// Activates a tutoring from a given tutor, student and exam.
  /// </summary>
  public void ActivateTutoring(int tutor, int student, int exam)
  {
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    const string query =
      "UPDATE tutor_to_exam as t SET available_tutorings = available_tutorings - 1, last_reservation = DEFAULT " +
      "WHERE exam = @examCode AND tutor = @tutorCode";

    // Find out if the same tutoring is already active
    const string query2 = "SELECT * FROM active_tutoring WHERE tutor=@tutorCode " +
                          "AND student=@studentCode AND duration IS NULL";
    try
    {
      var command = new MySqlCommand(query, Connection, transaction);
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
        Connection.Close();
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

      Connection.Close();
    }

    Connection.Close();
  }

  /// <summary>
  /// Activates an OFA tutoring from a tutor for a given student.
  /// </summary>
  public void ActivateTutoring(int tutor, int student)
  {
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    // Find out if tutor is available for OFA
    const string query =
      "SELECT * FROM tutor WHERE tutor_code = @tutorCode AND OFA_available = 1 ";

    // Find out if the same tutoring is already active
    const string query2 = "SELECT * FROM active_tutoring WHERE tutor=@tutorCode " +
                          "AND student=@studentCode AND duration IS NULL";
    try
    {
      var command = new MySqlCommand(query, Connection, transaction);
      command.Parameters.AddWithValue("@tutorCode", tutor);
      command.Prepare();
      var result = command.ExecuteScalar();
      if (result == null)
      {
        Log.Error($"Tried activating an OFA tutoring for tutor:{tutor} who isn't OFA available");
        Connection.Close();
        return;
      }

      command.CommandText = query2;
      command.Parameters.AddWithValue("@studentCode", student);
      command.Prepare();
      result = command.ExecuteScalar();
      if (result != null)
      {
        Log.Error($"Tried activating an already active tutoring for tutor: {tutor} and student: {student}");
        Connection.Close();
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

      Connection.Close();
    }

    Connection.Close();
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
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    const string query =
      "UPDATE active_tutoring SET end_date = CURRENT_TIMESTAMP, duration = @duration WHERE ID = @id AND student = @studentCode";
    try
    {
      var command = new MySqlCommand(query, Connection, transaction);
      command.Parameters.AddWithValue("@id", id);
      command.Parameters.AddWithValue("@duration", duration);
      command.Prepare();
      var affectedRows = command.ExecuteNonQuery();
      if (affectedRows == 0)
      {
        transaction.Commit();
        Connection.Close();
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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return true;
  }

  /// <summary>
  /// Ends tutorings by adding an end date to an active tutoring and setting the duration.
  /// if the tutoring wasn't for OFA it updates the number of available tutorings for the relative exam.
  /// </summary>
  /// <param name="tutoringToDurationList">list of tutoring to duration associations.</param>
  public bool EndTutorings(List<TutoringToDuration> tutoringToDurationList)
  {
    Connection.Open();
    var transaction = Connection.BeginTransaction();
    const string query =
      "UPDATE active_tutoring SET end_date = CURRENT_TIMESTAMP, duration = @duration WHERE ID = @id";
    try
    {
      foreach (var (id, duration) in tutoringToDurationList)
      {
        var command = new MySqlCommand(query, Connection, transaction);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@duration", duration);
        command.Prepare();
        var affectedRows = command.ExecuteNonQuery();
        if (affectedRows == 0)
        {
          transaction.Commit();
          Connection.Close();
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
        var isOfa = reader.GetBoolean("is_OFA");

        if (isOfa)
        {
          reader.Close();
          continue;
        }

        var exam = reader.GetInt32("exam");

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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return true;
  }

  /// <summary>
  /// Finds all OFA tutors that don't already have an active tutoring.
  /// The tutors are given in ranking order.
  /// </summary>
  /// <param name="lockHours">The amount of hours that need to have passed before a tutor isn't locked anymore.</param>
  /// <returns>List of available tutorings.</returns>
  public List<TutorToExam> FindAvailableOFATutors(int lockHours)
  {
    Connection.Open();
    // Finds all OFA available tutorings that don't 
    const string query = "SELECT * FROM tutor " +
                         "WHERE OFA_available = 1 AND tutor_code NOT IN " +
                         "(SELECT tutor FROM active_tutoring " +
                         "WHERE end_date IS NULL) " +
                         "ORDER BY ranking ASC";
    var tutors = new List<TutorToExam>();
    try
    {
      var command = new MySqlCommand(query, Connection);
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
      Connection.Close();
      throw;
    }

    Connection.Close();
    return tutors;
  }
}