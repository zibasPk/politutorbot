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

public record struct Reservation(
    int Id,
    int Tutor,
    int Exam,
    int Student,
    DateTime ReservationTimestamp,
    bool IsProcessed,
    bool IsOFA
);

public record struct Exam(
    int Code, 
    string Course, 
    string Name, 
    string Year
);
