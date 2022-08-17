namespace Bot.Enums;

public enum UserState
{
    Start = 0,
    School = 1,
    Course = 2,
    Year = 3,
    Exam = 4,
    Link = 5,
    ReLink = 999,
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
    var states = Enum.GetValues<UserState>();
    
    foreach (var state in states)
    {
        if (state == currentUserState - 1)
            return state;
    }

    return currentUserState;
}
    
}


