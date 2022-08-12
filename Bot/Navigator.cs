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
    /// <param name="school"></param>
    /// <returns>null if no course is found </returns>
    public static ReplyKeyboardMarkup? GenerateCourseKeyboard(string school)
    {
        if (!SchoolToCourses.TryGetValue(school, out var courses))
            return null;
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        for (int i = 0; i < courses.Length; i++)
        {
            row.Add(new KeyboardButton(courses[i]));
            if ((i + 1) % 3 == 0 || i == courses.Length - 1)
            {
                buttons.Add(row);
                row = new List<KeyboardButton>();
            }
        }
        return new ReplyKeyboardMarkup(buttons);
    }
}