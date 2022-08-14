using Bot.Enums;

namespace Bot;

public class Conversation
{
    public UserState State { get; set; }
    public string? School { get; set; }
    public string? Course { get; set; }
    public string? Year { get; set; }
    public string? Exam { get; set; }

    public Conversation()
    {
        this.State = UserState.Start;
    }

    public void GoToPreviousState()
    {
        switch (State)
        {
            case UserState.Start:
                break;
            case UserState.School:
                School = "";
                break;
            case UserState.Course:
                Course = "";
                break;
            case UserState.Year:
                Year = "";
                break;
            case UserState.Exam:
                Exam = "";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        State = State.GetPreviousState();
    }

    public string GetCurrentTopic()
    {
        return State switch
        {
            UserState.Start => "/start",
            UserState.School => School!,
            UserState.Course => Course!,
            UserState.Year => Year!,
            UserState.Exam => Exam!,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}