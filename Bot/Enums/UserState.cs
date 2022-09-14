namespace Bot.Enums;

public enum UserState
{
    Start,
    School,
    Course,
    Link,
    ReLink,
    OFA,
    OFATutor,
    Year,
    Exam,
    Tutor
}

public static class UserStateMethods
{
    /// <summary>
    /// Looks for the state preceding currentState
    /// </summary>
    /// <param name="currentUserState">The state for which it searches the previous</param>
    /// <returns>previous state if found; otherwise current state.</returns>
    public static UserState GetPreviousState(this UserState currentUserState)
    {
        
        return currentUserState switch
        {
            UserState.Start => UserState.Start,
            UserState.School => UserState.Start,
            UserState.Course => UserState.School,
            UserState.ReLink => UserState.Course,
            UserState.Link => UserState.Course,
            UserState.OFA => UserState.Link,
            UserState.OFATutor => UserState.Link,
            UserState.Year => UserState.Link,
            UserState.Exam => UserState.Year,
            UserState.Tutor => UserState.Exam,
            _ => currentUserState
        };
        
    }
}