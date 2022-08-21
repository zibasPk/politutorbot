namespace Bot.Enums;

public enum UserState
{
    Start,
    School,
    Course,
    Year,
    Exam,
    Link,
    ReLink,
}

public static class UserStateMethods
{
    private static int GetValue(this UserState currentUserState)
    {
        return currentUserState switch
        {
            UserState.Start => 0,
            UserState.School => 1,
            UserState.Course => 2,
            UserState.Year => 3,
            UserState.Exam => 4,
            UserState.Link => 5,
            UserState.ReLink => 99,
            _ => throw new ArgumentOutOfRangeException(nameof(currentUserState), currentUserState, null)
        };
    }

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
            if (state.GetValue() == currentUserState.GetValue() - 1)
                return state;
        }

        return currentUserState;
    }
}