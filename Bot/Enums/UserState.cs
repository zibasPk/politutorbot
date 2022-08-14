namespace Bot.Enums;

public enum State
{
    Start = 0,
    School = 1,
    Course = 2,
    Year = 3,
    Exam = 4
}

public static class StateMethods
{
    /// <summary>
    /// Looks for the state preceding currentState
    /// </summary>
    /// <param name="currentState">The state for which it searches the previous</param>
    /// <returns>previous state if found; otherwise current state.</returns>
public static State GetPreviousState(this State currentState)
{
    var states = Enum.GetValues<State>();
    
    foreach (var state in states)
    {
        if (state == currentState - 1)
            return state;
    }

    return currentState;
}

}


