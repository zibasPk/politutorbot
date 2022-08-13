using Bot.Enums;

namespace Bot;

public class Conversation
{
    public State State { get; set; }
    public string? School { get; set; }
    public string? Course { get; set; }
    public string? Year { get; set; }
    public string? Subject { get; set; }

    public Conversation()
    {
        this.State = State.Start;
    }
}