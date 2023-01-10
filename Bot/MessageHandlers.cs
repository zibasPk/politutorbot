using System.Text.RegularExpressions;
using Bot.configs;
using Bot.Constants;
using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
using Bot.Enums;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Bot.AsyncResponseHandler;

namespace Bot;

public static class MessageHandlers
{
  private static readonly Dictionary<long, Conversation> UserIdToConversation = new();

  public static async void OnApiCall(int userId)
  {
    UserIdToConversation.TryGetValue(userId, out var conversation);
    if (conversation == null)
    {
      Log.Warning("Received api call from user: {userId} with no active conversation", userId);
      return;
    }

    if (!conversation.WaitingForApiCall)
    {
      Log.Warning("Received api call from conversation with user: {userId} " +
                  "that wasn't waiting for an api call", userId);
      return;
    }

    Log.Debug("Received api call from user: {userId}", userId);
    lock (conversation.ConvLock)
    {
      conversation.WaitingForApiCall = false;
      // TODO: change db and application logic for person code
    }

    await SendMessage(conversation.ChatId, "Identità confermata con successo");
  }

  public static async Task HandleMessage(ITelegramBotClient botClient, Message message)
  {
    var chatId = message.Chat.Id;
    if (string.IsNullOrEmpty(message.Text))
    {
      Log.Warning("Received message with no text in chat {chatId}", chatId);
      return;
    }

    if (message.From == null)
    {
      Log.Warning("Received message '{text}' in chat {chatId} with no attached User", message.Text, chatId);
      return;
    }

    var userId = message.From.Id;
    Log.Information("Received message '{text}' by user {userId}", message.Text, userId);


    if (!UserIdToConversation.TryGetValue(userId, out var conversation))
    {
      conversation = new Conversation(chatId);
      UserIdToConversation.Add(userId, conversation);
    }

    var command = message.Text;
    if (command == "indietro")
    {
      if (conversation.WaitingForApiCall)
        conversation.WaitingForApiCall = false;

      if (conversation.State == UserState.Exam)
      {
        //this is necessary because of bad design; the whole state machine should be redone
        await SendYearKeyboard(botClient, message);
        return;
      }

      conversation.GoToPreviousState();
      conversation.GoToPreviousState();
      command = conversation.GetCurrentTopic();
    }

    try
    {
      var action = conversation.State switch
      {
        UserState.Start => SendSchoolKeyboard(botClient, message),
        UserState.School => SendCourseKeyboard(botClient, message, command!),
        UserState.Course => SendSaveUserData(botClient, message, command!),
        UserState.Link => ReadStudentCode(botClient, message, command!),
        UserState.ReLink => ReadReLinkAnswer(botClient, message, command!),
        UserState.OFA => ReadOFAAnswer(botClient, message, command!),
        UserState.OFATutor => ReadOFATutor(botClient, message, command!),
        UserState.Year => SendExamKeyboard(botClient, message, command!),
        UserState.Exam => SendTutorsKeyboard(botClient, message, command!),
        UserState.Tutor => ReadTutor(botClient, message, command!),
        _ => SendEcho(botClient, message)
      };

      await action;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      if (Monitor.IsEntered(conversation.ConvLock))
        Monitor.Exit(conversation.ConvLock);
      await InternalErrorMessage(botClient, message);
    }
  }

  private static async Task<Message> ReadOFATutor(ITelegramBotClient botClient, Message message, string tutor)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);
    var userService = new UserDAO(DbConnection.GetMySqlConnection());
    // Check if user has an already ongoing tutoring
    if (userService.HasUserOngoingTutoring(userId))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.AlreadyOngoingTutoring,
        replyMarkup: new ReplyKeyboardRemove());
    }

    // Check if user is already locked for finalizing a tutor request
    if (userService.IsUserLocked(userId, GlobalConfig.BotConfig!.TutorLockHours))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.LockedUser,
        replyMarkup: new ReplyKeyboardRemove());
    }

    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());

    var tutorCode = tutorService.FindTutorCode(tutor);

    //TODO: also check if tutor is for OFA
    if (tutorCode == null)
    {
      Log.Debug("Invalid {tutor} chosen in chat {id}.", tutor, message.Chat.Id);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidTutor);
    }

    // lock tutor until email arrive
    tutorService.ReserveOFATutor(tutorCode.Value, userId, conversation!.StudentCode);

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.TutorSelected,
      replyMarkup: new ReplyKeyboardRemove());
  }

  /// <summary>
  /// Reads yes/no and replies with the relative message: (tutor list and keyboard/year keyboard).
  /// </summary>
  private static async Task<Message> ReadOFAAnswer(ITelegramBotClient botClient, Message message, string command)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);

    var studentCode = conversation!.StudentCode;
    switch (command)
    {
      case "Sì":
      case "Si":
      case "si":
      case "SI":
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
          return await SendEcho(botClient, message);
        // Release lock from conversation
        Monitor.Exit(conversation.ConvLock);

        return await SendOFATutorsKeyboard(botClient, message);
      case "No":
      case "no":
      case "NO":
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
          return await SendEcho(botClient, message);

        conversation.State = UserState.Year;
        // Release lock from conversation
        Monitor.Exit(conversation.ConvLock);
        return await SendYearKeyboard(botClient, message);
      default:
        Log.Debug("Invalid {studentCode} chosen in chat {id}.", studentCode, message.Chat.Id);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
          text: ReplyTexts.InvalidStudentCode);
    }
  }

  private static async Task<Message> SendOFATutorsKeyboard(ITelegramBotClient botClient, Message message)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);
    // Check if conversation has been reset or is locked by a reset, if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
    {
      Log.Debug("user {id} tried to access a locked conversation", message.From.Id);
      return await SendEcho(botClient, message);
    }

    conversation.State = UserState.OFATutor;
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var tutors = tutorService.FindAvailableOFATutors(GlobalConfig.BotConfig!.TutorLockHours);
    if (tutors.Count == 0)
    {
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.NoOFATutoring,
        replyMarkup: KeyboardGenerator.BackKeyboard());
    }

    var keyboardMarkup = KeyboardGenerator.TutorKeyboard(tutors);
    var tutorsTexts = tutors.Select(x => "nome: " + x.Name + " " + x.Surname + "\ncorso: " + x.Course + "\n \n")
      .ToList();
    var text = ReplyTexts.SelectOFATutor;
    foreach (var tutorsText in tutorsTexts)
    {
      text += tutorsText;
    }

    Monitor.Exit(conversation.ConvLock);
    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: text,
      replyMarkup: keyboardMarkup);
  }

  /// <summary>
  /// Reads any message and replies with a school keyboard.
  /// </summary>
  private static async Task<Message> SendSchoolKeyboard(ITelegramBotClient botClient, Message message)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);

    var userService = new UserDAO(DbConnection.GetMySqlConnection());
    // Check if user has an already ongoing tutoring
    if (userService.HasUserOngoingTutoring(userId))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.AlreadyOngoingTutoring,
        replyMarkup: new ReplyKeyboardRemove());
    }

    // Check if user is already locked for finalizing a tutor request
    if (userService.IsUserLocked(userId, GlobalConfig.BotConfig!.TutorLockHours))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.LockedUser,
        replyMarkup: new ReplyKeyboardRemove());
    }

    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (!Monitor.TryEnter(conversation!.ConvLock))
    {
      Log.Debug("locked conversation");
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await SendEcho(botClient, message);
    }

    var replyKeyboardMarkup = KeyboardGenerator.SchoolKeyboard();
    if (replyKeyboardMarkup == null)
    {
      Log.Error("No schools found!.");
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InternalError);
    }

    // Change conversation state
    conversation.State = UserState.School;

    // Release lock on conversation
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("Sending School inline keyboard to chat: {id}.", message.Chat.Id);
    // Show typing action to client
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.SelectSchool,
      replyMarkup: replyKeyboardMarkup);
  }

  /// <summary>
  /// Reads a school and replies with correct course keyboard.
  /// </summary>
  private static async Task<Message> SendCourseKeyboard(ITelegramBotClient botClient, Message message, string school)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
      return await SendEcho(botClient, message);

    if (school != "ICAT")
    {
      Log.Debug("Unavailable school {school} chosen in chat {id}.", school, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.SchoolNotICAT);
    }

    var replyKeyboardMarkup = KeyboardGenerator.CourseKeyboard(school);

    // check school validity
    if (replyKeyboardMarkup == null)
    {
      Log.Debug("Invalid school {school} chosen in chat {id}.", school, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidSchool);
    }

    // Show typing action to client
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
    // Change conversation state to Course and save chosen school
    conversation.State = UserState.Course;
    conversation.School = school;

    // Release lock on conversation
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("Sending Course inline keyboard for school {school} to chat: {id}.", school, message.Chat.Id);
    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.SelectCourse,
      replyMarkup: replyKeyboardMarkup);
  }

  /// <summary>
  /// Replies with a year keyboard.
  /// </summary>
  private static async Task<Message> SendYearKeyboard(ITelegramBotClient botClient, Message message)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
      return await SendEcho(botClient, message);

    // Change conversation state to Year
    conversation.State = UserState.Year;
    // Release Lock on conversation
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("Sending Year inline keyboard to chat: {id}.", message.Chat.Id);
    // Show typing action to client
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

    var replyKeyboardMarkup = KeyboardGenerator.YearKeyboard();

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.SelectYear,
      replyMarkup: replyKeyboardMarkup);
  }

  private static async Task<Message> SendExamKeyboard(ITelegramBotClient botClient, Message message, string year)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
      return await SendEcho(botClient, message);
    // Check course validity
    var replyKeyboardMarkup = KeyboardGenerator.SubjectKeyboard(conversation.Course!, year);
    if (replyKeyboardMarkup == null)
    {
      Log.Debug("Invalid {year} chosen in chat {id}.", year, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidYear);
    }

    // Change conversation state to Subject and save chosen year
    conversation.State = UserState.Exam;
    conversation.Year = year;
    // Release lock on conversation
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("Sending Exam inline keyboard to chat: {id}.", message.Chat.Id);
    // Show typing action to client
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.SelectExam,
      replyMarkup: replyKeyboardMarkup);
  }

  /// <summary>
  /// Reads a course and replies with the proper person code insert prompt (link/relink).
  /// </summary>
  private static async Task<Message> SendSaveUserData(ITelegramBotClient botClient, Message message, string course)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
      return await SendEcho(botClient, message);
    var connection = DbConnection.GetMySqlConnection();

    // Check course validity
    var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
    if (!courseService.IsCourseInSchool(course, conversation.School!))
    {
      Log.Debug("Invalid {course} chosen in chat {id}.", course, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidCourse);
    }

    conversation.Course = course;
    conversation.State = UserState.Link;

    var userService = new UserDAO(connection);
    var userId = message.From.Id;
    var studentCode = userService.FindUserStudentCode(userId);
    if (studentCode == null)
    {
      // Check if Online Authentication is active
      if (GlobalConfig.BotConfig!.HasOnlineAuth)
      {
        conversation.WaitingForApiCall = true;
        Monitor.Exit(conversation.ConvLock);
        var text = "Login Aunica: " + GlobalConfig.BotConfig.AuthLink;
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
          text: text,
          replyMarkup: KeyboardGenerator.BackKeyboard());
      }

      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.LinkStudentCode,
        replyMarkup: new ReplyKeyboardRemove());
    }

    conversation.StudentCode = studentCode.Value;
    conversation.State = UserState.ReLink;
    // Release lock on conversation
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("User {userId} already linked to person code {studentCode}.", userId, studentCode);
    var replyKeyboardMarkup = KeyboardGenerator.YesOrNoKeyboard();
    return await
      botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.AlreadyLinkedAccount(studentCode.Value),
        replyMarkup: replyKeyboardMarkup);
  }

  /// <summary>
  /// Reads yes/no and replies with the relative message: (person code message prompt/ yes/no keyboard for OFA).
  /// </summary>
  private static async Task<Message> ReadReLinkAnswer(ITelegramBotClient botClient, Message message, string command)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);

    var studentCode = conversation!.StudentCode;
    var userService = new UserDAO(DbConnection.GetMySqlConnection());
    switch (command)
    {
      case "Sì":
      case "Si":
      case "si":
      case "SI":
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
          return await SendEcho(botClient, message);
        Log.Debug("User {userId} has chosen to delete association with person code {studentCode}"
          , userId, studentCode);
        userService.RemoveUser(userId);
        conversation.StudentCode = 0;
        conversation.State = UserState.Link;
        // Release lock from conversation

        // Check if Online Authentication is active
        if (!GlobalConfig.BotConfig!.HasOnlineAuth)
        {
          Monitor.Exit(conversation.ConvLock);
          return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: ReplyTexts.ReLinkStudentCode,
            replyMarkup: new ReplyKeyboardRemove());
        }

        conversation.WaitingForApiCall = true;
        Monitor.Exit(conversation.ConvLock);
        var text = "Login Aunica: " + GlobalConfig.BotConfig.AuthLink;
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
          text: text,
          replyMarkup: KeyboardGenerator.BackKeyboard());
      case "No":
      case "no":
      case "NO":
        // Check if conversation has been reset or is locked by a reset if not acquire lock
        if (conversation.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
          return await SendEcho(botClient, message);
        conversation.StudentCode = userService.FindUserStudentCode(userId)!.Value;
        // Release lock from conversation
        Monitor.Exit(conversation.ConvLock);

        return await SendOFAChoice(botClient, message);
      default:
        Log.Debug("Invalid {studentCode} chosen in chat {id}.", studentCode, message.Chat.Id);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
          text: ReplyTexts.InvalidStudentCode);
    }
  }

  private static async Task<Message> SendOFAChoice(ITelegramBotClient botClient, Message message)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
      return await SendEcho(botClient, message);
    conversation.State = UserState.OFA;
    Monitor.Exit(conversation.ConvLock);

    Log.Debug("Sending OFA choice inline keyboard to chat: {id}.", message.Chat.Id);
    // Show typing action to client
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

    var replyKeyboardMarkup = KeyboardGenerator.YesOrNoKeyboard();
    return await
      botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.OFAChoice,
        replyMarkup: replyKeyboardMarkup);
  }

  /// <summary>
  /// Reads a student code and replies with a yes/no keyboard for OFA tutoring.
  /// </summary>
  private static async Task<Message> ReadStudentCode(ITelegramBotClient botClient, Message message,
    string studentCodeStr)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    // Check if conversation has been reset or is locked by a reset if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
    {
      Log.Debug("user {id} tried to access a locked conversation", message.From.Id);
      return await SendEcho(botClient, message);
    }

    // Check if Online Authentication is active
    if (conversation.WaitingForApiCall)
    {
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: "Accedi ad Aunica per continuare.");
    }

    if (!Regex.IsMatch(studentCodeStr, "^[1-9][0-9]{5}$"))
    {
      Log.Debug("Invalid student number {studentCode} typed in chat {id}.", studentCodeStr, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidStudentCode);
    }

    var studentService = new StudentDAO(DbConnection.GetMySqlConnection());
    if (!studentService.IsStudentEnabled(int.Parse(studentCodeStr)))
    {
      Log.Debug("Invalid student number {studentCode} typed in chat {id}.", studentCodeStr, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.NotEnabledStudentCode,
        replyMarkup: KeyboardGenerator.BackKeyboard());
    }

    var studentCode = int.Parse(studentCodeStr);
    var userId = message.From!.Id;
    var userService = new UserDAO(DbConnection.GetMySqlConnection());
    userService.SaveUserLink(userId, studentCode);
    conversation.StudentCode = studentCode;
    // Release lock from conversation
    Monitor.Exit(conversation.ConvLock);

    return await SendOFAChoice(botClient, message);
  }

  private static async Task<Message> SendEcho(ITelegramBotClient botClient, Message message)
  {
    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: message.Text!);
  }

  private static async Task<Message> SendTutorsKeyboard(ITelegramBotClient botClient, Message message,
    string examName)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);

    // Check if conversation has been reset or is locked by a reset, if not acquire lock
    if (conversation!.State == UserState.Start || !Monitor.TryEnter(conversation.ConvLock))
    {
      Log.Debug("user {id} tried to access a locked conversation", message.From.Id);
      return await SendEcho(botClient, message);
    }


    var examService = new ExamDAO(DbConnection.GetMySqlConnection());
    var exam = examService.FindExam(examName, conversation.Course!);
    // Check if exam valid
    if (exam == null ||
        !examService.IsExamInCourse(exam.Value.Code, conversation.Course!, conversation.Year!))
    {
      Log.Debug("Invalid Exam {exam} chosen in chat {id}.", examName, message.Chat.Id);
      // Release lock from conversation
      Monitor.Exit(conversation.ConvLock);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidExam);
    }

    conversation.Exam = exam;
    conversation.State = UserState.Tutor;
    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var tutors = tutorService.FindAvailableTutors(exam.Value.Code, GlobalConfig.BotConfig!.TutorLockHours);
    if (tutors.Count == 0)
    {
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.NoTutoring,
        replyMarkup: KeyboardGenerator.BackKeyboard());
    }

    tutors = tutors.OrderBy(tutor => tutor.Course == conversation.Course ? 0 : 1)
      .ThenBy(tutor => tutor.Ranking)
      .ToList();

    var keyboardMarkup = KeyboardGenerator.TutorKeyboard(tutors);
    var tutorsTexts = tutors.Select(x => "nome: " + x.Name + " " + x.Surname + "\ncorso: " + x.Course +
                                         "\nprof: " + x.Professor + "\n \n")
      .ToList();
    var text = ReplyTexts.SelectTutor(conversation.Exam.Value.Name);
    foreach (var tutorsText in tutorsTexts)
    {
      text += tutorsText;
    }

    Monitor.Exit(conversation.ConvLock);
    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: text,
      replyMarkup: keyboardMarkup);
  }

  private static async Task<Message> ReadTutor(ITelegramBotClient botClient, Message message, string tutor)
  {
    var userId = message.From!.Id;
    UserIdToConversation.TryGetValue(userId, out var conversation);

    var userService = new UserDAO(DbConnection.GetMySqlConnection());
    // Check if user has an already ongoin tutoring
    if (userService.HasUserOngoingTutoring(userId))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.AlreadyOngoingTutoring,
        replyMarkup: new ReplyKeyboardRemove());
    }

    // Check if user is already locked for finalizing a tutor request
    if (userService.IsUserLocked(userId, GlobalConfig.BotConfig!.TutorLockHours))
    {
      return await botClient.SendTextMessageAsync(chatId: conversation!.ChatId,
        text: ReplyTexts.LockedUser,
        replyMarkup: new ReplyKeyboardRemove());
    }

    var tutorService = new TutorDAO(DbConnection.GetMySqlConnection());
    var exam = conversation!.Exam!.Value;
    var tutorCode = tutorService.FindTutorCode(tutor, exam.Code);
    if (tutorCode == null || !tutorService.IsTutorForExam(tutorCode.Value, exam.Code))
    {
      Log.Debug("Invalid {tutor} chosen in chat {id}.", tutor, message.Chat.Id);
      return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
        text: ReplyTexts.InvalidTutor);
    }

    // lock tutor until email arrive
    tutorService.ReserveTutor(tutorCode.Value, exam.Code, userId, conversation.StudentCode);

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.TutorSelected,
      replyMarkup: new ReplyKeyboardRemove());
  }

  private static async Task<Message> InternalErrorMessage(ITelegramBotClient botClient, Message message)
  {
    UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
    conversation!.ResetConversation();

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
      text: ReplyTexts.InternalError,
      replyMarkup: new ReplyKeyboardRemove());
  }
}

public static class AsyncResponseHandler
{
  private static ITelegramBotClient? _botClient;

  public static void Init(ITelegramBotClient botClient)
  {
    _botClient = botClient;
  }

  public static async Task SendMessage(long chatId, string text)
  {
    if (_botClient == null)
      throw new InvalidOperationException("AsyncResponder class was not initialized. " +
                                          "Call the Init() to initialize static botClient.");
    await _botClient.SendTextMessageAsync(chatId: chatId,
      text: text,
      replyMarkup: new ReplyKeyboardRemove());
  }

  public static async Task SendMessage(long chatId, string text, ITelegramBotClient botClient)
  {
    await botClient.SendTextMessageAsync(chatId: chatId,
      text: text,
      replyMarkup: new ReplyKeyboardRemove());
  }
}