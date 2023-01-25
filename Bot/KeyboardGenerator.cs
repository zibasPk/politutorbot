using Bot.Database;
using Bot.Database.Dao;
using Bot.Database.Records;
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
        var courses = courseService.FindCourses(school);
        return courses.Count == 0 ? null : GenerateKeyboardMarkup(courses, true);
    }

    /// <summary>
    /// Generate a keyboard containing given exams
    /// </summary>
    /// <param name="exams">exams to display on the keyboard</param>
    public static ReplyKeyboardMarkup ExamsKeyboard(List<Exam> exams)
    {
        var examsNames = exams.Select(x => x.Name).ToList();
        return GenerateKeyboardMarkup(examsNames, true);
    }

    public static ReplyKeyboardMarkup YearKeyboard(string course)
    {
        var courseService = new CourseDAO(DbConnection.GetMySqlConnection());
        var items = courseService.AvailableYearsInCourse(course);
        items.Sort();
        return GenerateKeyboardMarkup(items, 2, true);
    }

    public static ReplyKeyboardMarkup YesOrNoKeyboard()
    {
        var items = new List<string>() { "Si", "No" };
        return GenerateKeyboardMarkup(items, 2, true);
    }

    /// <summary>
    /// Generate a keyboard containing only a back button.
    /// </summary>
    /// <returns></returns>
    public static ReplyKeyboardMarkup BackKeyboard()
    {
        return GenerateKeyboardMarkup(new List<string>(), true);
    }

    public static ReplyKeyboardMarkup TutorKeyboard(List<TutorToExam> tutors)
    {
        var numbers = tutors.Select(tutor => (tutors.IndexOf(tutor) + 1).ToString()).ToList();
        
        return GenerateKeyboardMarkup(numbers, true);
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