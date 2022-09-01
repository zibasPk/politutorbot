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
    int AvailableReservations
);

public record struct Exam(
    int Code, 
    string Course, 
    string Name, 
    string Year
);
