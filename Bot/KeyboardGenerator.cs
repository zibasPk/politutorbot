using Bot.Database;
using Bot.Database.Dao;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public static class KeyboardGenerator
{
    /// <summary>
    /// Generate a keyboard containing available schools
    /// </summary>
    /// <returns>null if no School is found </returns>
    public static ReplyKeyboardMarkup? GenerateSchoolKeyboard()
    {
        var schoolService = new SchoolDAO(DbConnection.GetMySqlConnection());
        var schools = schoolService.FindSchools();
        if (schools.Count == 0)
            return null;
        return GenerateKeyboardMarkup(schools, 2, false);
    }

    /// <summary>
    /// Generate a keyboard containing courses for a specific school
    /// </summary>
    /// <param name="school">The School for which it generates the keyboard</param>  
    /// <returns>null if no course is found </returns>
    public static ReplyKeyboardMarkup? GenerateCourseKeyboard(string school)
    {
        var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
        var courses = courseService.FindCoursesInSchool(school);
        if (courses.Count == 0)
            return null;
        return GenerateKeyboardMarkup(courses, 3, true);
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
        return GenerateKeyboardMarkup(exams, 3, true);
    }

    public static ReplyKeyboardMarkup GenerateYearKeyboard()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton[] { "Y1", "Y2" },
                new KeyboardButton[] { "Y3" },
                new KeyboardButton[] { "indietro" }
            })
        {
            ResizeKeyboard = true
        };
    }

    public static ReplyKeyboardMarkup GenerateYesOrNoKeyboard()
    {
        return new ReplyKeyboardMarkup(
            new KeyboardButton[] { "Si", "No" }
        )
        {
            ResizeKeyboard = true
        };
    }

    private static ReplyKeyboardMarkup GenerateKeyboardMarkup(List<string> items, int itemsPerRow, bool hasBackButton)
    {
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        for (var i = 0; i < items.Count; i++)
        {
            row.Add(new KeyboardButton(items[i]));
            if ((i + 1) % itemsPerRow != 0 && i != items.Count - 1)
                continue;
            buttons.Add(row);
            row = new List<KeyboardButton>();
        }

        if (hasBackButton)
            buttons.Add(new List<KeyboardButton>() { new("indietro") });
        return new ReplyKeyboardMarkup(buttons);
    }
}