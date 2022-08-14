using System.Collections;
using Bot.Database;
using Bot.Database.Dao;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public static class Navigator
{
    private static readonly Dictionary<string, string[]> SchoolToCourses = new()
    {
        ["3I"] = new[]
        {
            "MatNano",
            "Info",
            "MobilityMD",
            "AES",
            "Electronics",
            "Automazione",
            "Chimica",
            "Elettrica",
            "Meccanica",
            "Energetica"
        },
        ["AUIC"] = Array.Empty<string>(),
        ["ICAT"] = new[]
        {
            "Civile",
            "Ambientale",
            "Territoriale",
        },
        ["Design"] = Array.Empty<string>()
    };
    
    /// <summary>
    /// Generate a keyboard containing courses for a specific school
    /// </summary>
    /// <param name="school">The School for which it generates the keyboard</param>  
    /// <returns>null if no course is found </returns>
    public static ReplyKeyboardMarkup? GenerateCourseKeyboard(string school)
    {
        if (!SchoolToCourses.TryGetValue(school, out var courses))
            return null;
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        for (var i = 0; i < courses.Length; i++)
        {
            row.Add(new KeyboardButton(courses[i]));
            if ((i + 1) % 3 != 0 && i != courses.Length - 1) 
                continue;
            buttons.Add(row);
            row = new List<KeyboardButton>();
        }
        buttons.Add(new List<KeyboardButton>(){ new ("indietro")});
        return new ReplyKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Generate a keyboard containing Subjects for a specific course in a year
    /// </summary>
    /// <param name="course">The course for which it generates the keyboard</param>  
    /// <returns>null if course doesn't exist </returns>
    public static ReplyKeyboardMarkup? GenerateSubjectKeyboard(string course, string year)
    {
        var examService = new ExamDAO(DbConnection.GetMySqlConnection());
        var exams = examService.FindExamsInYear(course, year);
        if (exams.Count == 0)
            return null;
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        for (var i = 0; i < exams.Count; i++)
        {
            row.Add(new KeyboardButton(exams[i]));
            if ((i + 1) % 3 != 0 && i != exams.Count - 1) continue;
            buttons.Add(row);
            row = new List<KeyboardButton>();
        }
        
        buttons.Add(new List<KeyboardButton>(){ new ("indietro")});
        return new ReplyKeyboardMarkup(buttons);
    }
    public static ReplyKeyboardMarkup? GenerateYearKeyboard(string course)
    {
        
        return new(
            new[]
            {
                new KeyboardButton[] { "Y1", "Y2" },
                new KeyboardButton[] { "Y3" },
            })
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Determines whether a School contains a specified Course 
    /// </summary>
    /// <param name="course">The Course to check</param> 
    /// <param name="school">The School in which to check</param> 
    /// <returns>true if the specified School contains the given Course; otherwise, false</returns>
    public static bool IsCourseInSchool(string course, string school)
    {
        SchoolToCourses.TryGetValue(school, out var courses);
        if (courses == null)
            return false;
        return courses.Contains(course);
    }

    
}