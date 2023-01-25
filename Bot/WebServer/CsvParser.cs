using System.Globalization;
using Bot.Database.Records;
using CsvHelper;
using CsvHelper.Configuration;

namespace Bot.WebServer;

public static class CsvParser
{
    public static async Task<List<TutorToExam>> ParseTutors(Stream cvsStream)
    {
        TextReader reader = new StreamReader(cvsStream);
        var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        csvReader.Context.RegisterClassMap<CsvTutorMap>();
        var tutors = csvReader.GetRecordsAsync<TutorToExam>();
        
        return await tutors.ToListAsync();
    }

    private sealed class CsvTutorMap : ClassMap<TutorToExam>
    {
        public CsvTutorMap()
        {
            // Map(x => x.Name).Index(0);
            // Map(x => x.Course).Index(1);
            // Map(x => x.School).Index(2);
            // Map(x => x.Ranking).Index(3);
            // Map(x => x.Exam).Index(4);
        }
    }
}