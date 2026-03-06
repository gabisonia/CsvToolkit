using CsvToolkit.Core;
using CsvToolkit.Sample.Models;

namespace CsvToolkit.Sample.Demos;

public static class WriteApiDemo
{
    public static void Run(string outputPath, CsvOptions options)
    {
        Console.WriteLine("\n[9] Write API (WriteRecords + WriteField + NextRecord)");
        var records = BuildRecords();

        using var stream = File.Create(outputPath);
        using var writer = new CsvWriter(stream, options);

        writer.WriteRecords(records, writeHeader: true);

        writer.WriteField("manual_note");
        writer.WriteField("this row was added with WriteField");
        writer.NextRecord();

        writer.Flush();

        Console.WriteLine($"Wrote file: {outputPath}");
    }

    private static IReadOnlyList<AttributedPerson> BuildRecords()
    {
        return
        [
            new AttributedPerson
            {
                Id = 10,
                Name = "Nina Brooks",
                Email = "nina@example.com",
                Age = 30,
                BirthDate = new DateOnly(1995, 4, 18),
                IgnoredAtRuntime = "hidden"
            },
            new AttributedPerson
            {
                Id = 11,
                Name = "Owen Price",
                Email = "owen@example.com",
                Age = 42,
                BirthDate = new DateOnly(1983, 11, 2),
                IgnoredAtRuntime = "hidden"
            }
        ];
    }
}