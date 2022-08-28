namespace Bot.Database.Entity;

public record struct Tutoring(
    string Name, 
    string Exam, 
    string Course, 
    string School,
    int Ranking,
    DateTime LockTimeStamp,
    long LockedBy
    );
