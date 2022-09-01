using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Entity;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public static class KeyboardGenerator
{
    /// <summary>
    /// Generate a keyboard containing available schools.
    /// </summary>
    /// <returns>null if no School is found.</returns>
    public static ReplyKeyboardMarkup? SchoolKeyboard()
    {
        var schoolService = new SchoolDAO(DbConnection.GetMySqlConnection());
        var schools = schoolService.FindSchools();
        return schools.Count == 0 ? null : GenerateKeyboardMarkup(schools, 2, false);
    }

    /// <summary>
    /// Generate a keyboard containing courses for a specific school.
    /// </summary>
    /// <param name="school">The School for which it generates the keyboard.</param>  
    /// <returns>null if no course is found.</returns>
    public static ReplyKeyboardMarkup? CourseKeyboard(string school)
    {
        var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
        var courses = courseService.FindCoursesInSchool(school);
        return courses.Count == 0 ? null : GenerateKeyboardMarkup(courses, true);
    }

    /// <summary>
    /// Generate a keyboard containing Subjects for a specific course in a year.
    /// </summary>
    /// <param name="course">The course for which it generates the keyboard.</param>
    /// <param name="year">The year for which to look for exams.</param>
    /// <returns>null if course doesn't exist.</returns>
    public static ReplyKeyboardMarkup? SubjectKeyboard(string course, string year)
    {
        var examService = new ExamDAO(DbConnection.GetMySqlConnection());
        var exams = examService.FindExamsInYear(course, year);
        var examsNames = exams.Select(x => x.Name).ToList();
        return exams.Count == 0 ? null : GenerateKeyboardMarkup(examsNames, true);
    }

    public static ReplyKeyboardMarkup YearKeyboard()
    {
        var items = new List<string>() { "Y1", "Y2", "Y3" };
        return GenerateKeyboardMarkup(items, 2, true);
    }

    public static ReplyKeyboardMarkup YesOrNoKeyboard()
    {
        var items = new List<string>() { "Si", "No" };
        return GenerateKeyboardMarkup(items, 2, true);
    }

    public static ReplyKeyboardMarkup BackKeyboard()
    {
        return GenerateKeyboardMarkup(new List<string>(), true);
    }

    public static ReplyKeyboardMarkup TutorKeyboard(List<TutorToExam> tutors)
    {
        var names = tutors.Select(t => t.Name + " " + t.Surname).ToList();
        return GenerateKeyboardMarkup(names, true);
    }

    /// <summary>
    /// Generates a ReplyKeyboardMarkup dynamically by having a maximum of characters per row.
    /// <br/> The maximum items per row are 3.
    /// </summary>
    /// <param name="items">List of texts to put in buttons.</param>
    /// <param name="hasBackButton">true if markup needs to be generated with a back button; otherwise false.</param>
    /// <returns>Generated ReplyKeyboardMarkup.</returns>
    public static ReplyKeyboardMarkup GenerateKeyboardMarkup(List<string> items, bool hasBackButton)
    {
        const int maxCharsPerRow = 87;
        const int maxItemsPerRow = 3;
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        var characterCount = 0;
        for (var i = 0; i < items.Count; i++)
        {
            characterCount += items[i].Length;
            if (characterCount > maxCharsPerRow || row.Count >= maxItemsPerRow)
            {
                buttons.Add(row);
                characterCount = items[i].Length;
                row = new List<KeyboardButton> { new(items[i]) };
            }
            else
            {
                row.Add(new KeyboardButton(items[i]));
            }

            if (i == items.Count - 1)
                buttons.Add(row);
        }

        if (hasBackButton)
            buttons.Add(new List<KeyboardButton>() { new("indietro") });
        return new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Generates a ReplyKeyboardMarkUp with the requested amount of items per row.
    /// </summary>
    /// <param name="items">List of texts to put in buttons.</param>
    /// <param name="itemsPerRow">Max amount of items in one row.</param>
    /// <param name="hasBackButton">true if markup needs to be generated with a back button; otherwise false.</param>
    /// <returns>Generated ReplyKeyboardMarkup.</returns>
    public static ReplyKeyboardMarkup GenerateKeyboardMarkup(List<string> items, int itemsPerRow, bool hasBackButton)
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
        return new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };
    }
}