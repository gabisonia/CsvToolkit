using CsvToolkit.Mapping;
using CsvToolkit.TypeConversion;

namespace CsvToolkit.Tests.Mapping;

public sealed class CsvMappingTests
{
    [Fact]
    public void AttributeMapping_ReadsStronglyTypedRecord()
    {
        // Arrange
        const string csv = "identifier,full_name,status,score,created,ignored\n1,Ada,Active,42,2025-01-02,skip\n";
        using var reader = new CsvReader(new StringReader(csv));

        // Act
        var read = reader.Read();
        var person = reader.GetRecord<AttributedPerson>();

        // Assert
        Assert.True(read);
        Assert.Equal(1, person.Id);
        Assert.Equal("Ada", person.Name);
        Assert.Equal(PersonStatus.Active, person.Status);
        Assert.Equal(42, person.Score);
        Assert.Equal(new DateOnly(2025, 1, 2), person.Created);
        Assert.Null(person.Ignored);
    }

    [Fact]
    public void FluentMapping_ReadsByConfiguredNamesAndIndexes()
    {
        // Arrange
        const string csv = "first,last,years\nAda,Lovelace,36\n";
        var maps = new CsvMapRegistry();
        maps.Register<FluentPerson>(map =>
        {
            map.Map(x => x.FirstName).Name("first").Index(0);
            map.Map(x => x.LastName).Name("last").Index(1);
            map.Map(x => x.Age).Name("years").Index(2);
        });
        using var reader = new CsvReader(new StringReader(csv), mapRegistry: maps);

        // Act
        var read = reader.Read();
        var person = reader.GetRecord<FluentPerson>();

        // Assert
        Assert.True(read);
        Assert.Equal("Ada", person.FirstName);
        Assert.Equal("Lovelace", person.LastName);
        Assert.Equal(36, person.Age);
    }

    [Fact]
    public void FluentMapping_UsesCustomConverter()
    {
        // Arrange
        const string csv = "name\nada\n";
        var maps = new CsvMapRegistry();
        maps.Register<CustomNameRecord>(map =>
        {
            map.Map(x => x.Name).Name("name").Converter(new UpperCaseStringConverter());
        });
        using var reader = new CsvReader(new StringReader(csv), mapRegistry: maps);

        // Act
        var read = reader.Read();
        var record = reader.GetRecord<CustomNameRecord>();

        // Assert
        Assert.True(read);
        Assert.Equal("ADA", record.Name);
    }

    [Fact]
    public void NullableAndEnumConversion_AreSupported()
    {
        // Arrange
        const string csv = "id,status,score\n1,Inactive,\n";
        using var reader = new CsvReader(new StringReader(csv));

        // Act
        var read = reader.Read();
        var row = reader.GetRecord<NullableRecord>();

        // Assert
        Assert.True(read);
        Assert.Equal(1, row.Id);
        Assert.Equal(PersonStatus.Inactive, row.Status);
        Assert.Null(row.Score);
    }

    private sealed class UpperCaseStringConverter : ICsvTypeConverter<string>
    {
        public bool TryParse(ReadOnlySpan<char> source, in CsvConverterContext context, out string value)
        {
            value = source.ToString().ToUpperInvariant();
            return true;
        }

        public string Format(string value, in CsvConverterContext context)
        {
            return value.ToLowerInvariant();
        }
    }

    private enum PersonStatus
    {
        Active,
        Inactive
    }

    private sealed class AttributedPerson
    {
        [CsvColumn("identifier")] public int Id { get; set; }

        [CsvColumn("full_name")] public string Name { get; set; } = string.Empty;

        [CsvColumn("status")] public PersonStatus Status { get; set; }

        [CsvColumn("score")] public int? Score { get; set; }

        [CsvColumn("created")] public DateOnly Created { get; set; }

        [CsvIgnore] public string? Ignored { get; set; }
    }

    private sealed class FluentPerson
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    private sealed class CustomNameRecord
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class NullableRecord
    {
        public int Id { get; set; }

        public PersonStatus Status { get; set; }

        public int? Score { get; set; }
    }
}
