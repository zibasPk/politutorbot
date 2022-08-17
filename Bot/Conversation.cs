using Bot.Database.Dao;
using Bot.Enums;

namespace Bot;

public class Conversation
{
    public UserState State { get; set; }
    public string? School { get; set; }
    public string? Course { get; set; }
    public string? Year { get; set; }
    public string? Exam { get; set; }

    public int StudentNumber { get; set; }
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
                School = null;
                break;
            case UserState.Course:
                Course = null;
                break;
            case UserState.Year:
                Year = null;
                break;
            case UserState.Exam:
                Exam = null;
                break;
            case UserState.Link:
                StudentNumber = 0;
                break;
            case UserState.ReLink:
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
            UserState.Link => StudentNumber.ToString(),
            UserState.ReLink => StudentNumber.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}