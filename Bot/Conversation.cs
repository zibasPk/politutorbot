using System.Timers;
using Bot.configs;
using Bot.Constants;
using Bot.Database.Records;
using Bot.Enums;
using Org.BouncyCastle.Crypto.Modes;
using Serilog;
using Timer = System.Timers.Timer;

namespace Bot;

/// <summary>
/// Class containing data and useful methods
/// related to a conversation between the bot and a user.
/// </summary>
public class Conversation
{
    public bool WaitingForApiCall = false;
    public readonly object ConvLock = new();
    public DateTime LastMessage;
    private readonly Timer _conversationTimer;
    private UserState _state;

    public UserState State
    {
        get => _state;
        set
        {
            // Reset timer on state change
            if (value != UserState.Start)
                _conversationTimer.Interval = GlobalConfig.BotConfig!.UserTimeOut;
            _state = value;
        }
    }

    public long ChatId { get; set; }
    public string? School { get; set; }
    public string? Course { get; set; }
    public string? Year { get; set; }
    public Exam? Exam { get; set; }
    public int StudentCode { get; set; }

    public List<TutorToExam>? ShownTutors { get; set; }
    public List<Exam>? ShownExams { get; set; }
    public string? Tutor { get; set; }

    public Conversation(long chatId)
    {
        ChatId = chatId;
        State = UserState.Start;
        LastMessage = DateTime.MinValue;
        _conversationTimer = new Timer(GlobalConfig.BotConfig!.UserTimeOut);
        _conversationTimer.Elapsed += ResetConversation;
        _conversationTimer.AutoReset = false;
        _conversationTimer.Start();
    }

    /// <summary>
    /// Changes State to previous and erases the data related to that state.
    /// </summary>
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
            case UserState.Link:
                StudentCode = 0;
                break;
            case UserState.ReLink:
                break;
            case UserState.OFA:
                break;
            case UserState.OFATutor:
                break;
            case UserState.Year:
                Year = null;
                ShownExams = null;
                break;
            case UserState.Exam:
                Exam = null;
                ShownTutors = null;
                break;
            case UserState.Tutor:
                Tutor = null;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        State = State.GetPreviousState();
    }

    /// <returns>The topic attaining to the current State;<br/>null if topic hasn't been set</returns>
    public string? GetCurrentTopic()
    {
        return State switch
        {
            UserState.Start => "/start",
            UserState.School => School,
            UserState.Course => Course,
            UserState.Year => Year,
            UserState.Exam => Exam!.Value.Name,
            UserState.Link => StudentCode != 0 ? StudentCode.ToString() : null,
            UserState.ReLink => StudentCode != 0 ? StudentCode.ToString() : null,
            UserState.Tutor => Tutor,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Resets a conversation after time out by erasing all data but the chatId. 
    /// </summary>
    private async void ResetConversation(object? source, ElapsedEventArgs e)
    {
        if (!Monitor.TryEnter(ConvLock))
        {
            Log.Debug("Tried clearing locked conversation... resetting timer ");
            _conversationTimer.Interval = GlobalConfig.BotConfig!.UserTimeOut;
            return;
        }
        Log.Debug("Resetting conversation in state {state}", State);
        State = UserState.Start;
        School = null;
        Course = null;
        Year = null;
        Exam = null;
        StudentCode = 0;
        Tutor = null;
        ShownExams = null;
        ShownTutors = null;
        Monitor.Exit(ConvLock);
        await AsyncResponseHandler.SendMessage(ChatId, ReplyTexts.ConversationReset);
    }

    /// <summary>
    /// Resets a conversation by erasing all data but the chatId. 
    /// </summary>
    public async void ResetConversation()
    {
        if (!Monitor.TryEnter(ConvLock))
            return;
        Log.Debug("Resetting conversation in state {state}", State);
        State = UserState.Start;
        School = null;
        Course = null;
        Year = null;
        Exam = null;
        StudentCode = 0;
        Tutor = null;
        Monitor.Exit(ConvLock);
    }
}