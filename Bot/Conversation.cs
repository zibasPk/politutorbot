using Bot.Enums;

namespace Bot;

public class Conversation
{
    public State State { get; set; }

    public School? School { get; set; }
    
    public Year? Year { get; set; }

    public Conversation()
    {
        this.State = State.Start;
    }
}