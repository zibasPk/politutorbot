namespace Bot.Database.Records;

public record struct Tutor(
  int TutorCode,
  string Name,
  string Surname,
  string Course,
  bool OfaAvailable,
  int Ranking,
  int? ContractState,
  int? HoursDone
);

public record struct TutorToExam(
  int TutorCode,
  string Name,
  string Surname,
  int ExamCode,
  string? ExamName,
  string Course,
  string? School,
  string Professor,
  int Ranking,
  bool OfaAvailable,
  DateTime? LastReservation,
  int AvailableTutorings,
  int? ContractState
);

public record struct TutorCodeToExamCode
(
  int TutorCode,
  int ExamCode
);

public record struct ActiveTutoring(
  int Id,
  int TutorCode,
  string TutorName,
  string TutorSurname,
  int? ExamCode,
  int StudentCode,
  bool IsOFA,
  DateTime StartDate,
  DateTime? EndDate,
  int Duration
);

public record Reservation
{
  public int Id;
  public int Tutor;
  public int? Exam;
  public int Student;
  public DateTime ReservationTimestamp;
  public bool IsProcessed;
  public bool IsOFA;
}

public record ExtendedReservation : Reservation
{
  public string? TutorName;
  public string? TutorSurname;
}

public record struct Exam(
  int Code,
  string Course,
  string Name,
  string Year
);

public record struct Course
(
  string Name,
  string School
);

public record struct TutoringToDuration(int Id, int Duration);

public record struct TutorToStudentToExam(bool IsOFA, int TutorCode, int StudentCode, int? ExamCode);