using CsvToolkit.Core;
using CsvToolkit.Core.Mapping;

namespace CsvToolkit.Sample.Demos;

public static class AdvancedAttributeApiDemo
{
    public static void Run()
    {
        Console.WriteLine("\n[6] Attribute extras (CsvNameIndex + CsvOptional + CsvDefault + CsvConstant)");

        const string csv = """
                           name,name,age,country
                           Ada,Lovelace,,FR
                           """;

        using var reader = new CsvReader(new StringReader(csv));

        foreach (var record in reader.GetRecords<AdvancedAttributedPerson>())
        {
            Console.WriteLine(
                $"attr+: {record.FirstName} {record.LastName} | Age={record.Age} | Missing={record.Missing} | Country={record.Country}");
        }
    }

    private sealed class AdvancedAttributedPerson
    {
        [CsvColumn("name"), CsvNameIndex(0)] public string FirstName { get; set; } = string.Empty;

        [CsvColumn("name"), CsvNameIndex(1)] public string LastName { get; set; } = string.Empty;

        [CsvColumn("missing"), CsvOptional] public int Missing { get; set; }

        [CsvColumn("age"), CsvDefault(18)] public int Age { get; set; }

        [CsvColumn("country"), CsvConstant("US")]
        public string Country { get; set; } = string.Empty;
    }
}