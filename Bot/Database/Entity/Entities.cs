namespace Bot.Database.Entity;

public record struct TutorToExam(
    int TutorCode,
    string Name,
    string Surname,
    int ExamCode,
    string Course,
    string Professor,
    int Ranking,
    bool OfaAvailable,
    DateTime LastReservation,
    int AvailableTutorings
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
};

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