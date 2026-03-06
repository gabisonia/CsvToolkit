using System.Globalization;
using CsvToolkit.Core;

namespace CsvToolkit.Sample.Demos;

public static class EnumerableApiDemo
{
    public static void Run()
    {
        Console.WriteLine("\n[4] Enumerable API (GetRecords + DetectDelimiter + PrepareHeaderForMatch)");

        const string csv = """
                           Person Id ; Full Name ; Is Active ; Score
                           1 ; Ada Lovelace ; Y ; NULL
                           2 ; Grace Hopper ; N ; 42
                           """;

        var options = new CsvOptions
        {
            DetectDelimiter = true,
            DelimiterCandidates = [",", ";", "\t", "|"],
            TrimOptions = CsvTrimOptions.Trim,
            CultureInfo = CultureInfo.InvariantCulture,
            PrepareHeaderForMatch = static (header, _) =>
                header.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant(),
            HeaderValidated = context =>
                Console.WriteLine($"Headers: {string.Join(" | ", context.Headers)}")
        };

        options.ConverterOptions.Configure<bool>(converter => converter.AddTrueValues("Y").AddFalseValues("N"));
        options.ConverterOptions.Configure<int?>(converter => converter.AddNullValues("NULL"));

        using var reader = new CsvReader(new StringReader(csv), options);
        var records = reader.GetRecords<EnumerablePerson>().ToList();

        Console.WriteLine($"Detected delimiter: {reader.DetectedDelimiter}");
        foreach (var record in records)
        {
            Console.WriteLine(
                $"enum: {record.PersonId} {record.FullName} | Active={record.IsActive} | Score={record.Score?.ToString() ?? "<null>"}");
        }
    }

    private sealed class EnumerablePerson
    {
        public int PersonId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int? Score { get; set; }
    }
}