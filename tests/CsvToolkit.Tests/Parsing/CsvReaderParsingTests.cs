using System.Globalization;
using CsvToolkit.Mapping;

namespace CsvToolkit.Tests.Parsing;

public sealed class CsvReaderParsingTests
{
    [Fact]
    public void TryReadRow_HandlesQuotedDelimiter()
    {
        const string csv = "id,name\n1,\"Ada,Lovelace\"\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.TryReadRow(out var row));
        Assert.Equal("1", row.GetFieldString(0));
        Assert.Equal("Ada,Lovelace", row.GetFieldString(1));
    }

    [Fact]
    public void TryReadRow_HandlesEmbeddedNewLineInsideQuotes()
    {
        const string csv = "id,notes\n1,\"line1\nline2\"\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.TryReadRow(out var row));
        Assert.Equal("line1\nline2", row.GetFieldString(1));
    }

    [Fact]
    public void TryReadRow_HandlesEscapedQuotes()
    {
        const string csv = "id,text\n1,\"a \"\"quote\"\" b\"\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.TryReadRow(out var row));
        Assert.Equal("a \"quote\" b", row.GetFieldString(1));
    }

    [Fact]
    public void TryReadRow_SupportsCustomDelimiter()
    {
        const string csv = "id;name\n1;Ada\n";
        var options = new CsvOptions { Delimiter = ';' };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.TryReadRow(out var row));
        Assert.Equal("Ada", row.GetFieldString(1));
    }

    [Fact]
    public void TryReadDictionary_UsesHeaderNames()
    {
        const string csv = "id,name\n1,Ada\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.TryReadDictionary(out var values));
        Assert.NotNull(values);
        Assert.Equal("1", values["id"]);
        Assert.Equal("Ada", values["name"]);
    }

    [Fact]
    public void ReadAsync_ReadsRows()
    {
        const string csv = "id,name\n1,Ada\n2,Bob\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.ReadAsync().GetAwaiter().GetResult());
        Assert.Equal("Ada", reader.GetField(1));
        Assert.True(reader.ReadAsync().GetAwaiter().GetResult());
        Assert.Equal("Bob", reader.GetField(1));
        Assert.False(reader.ReadAsync().GetAwaiter().GetResult());
    }

    [Fact]
    public void TrimOptions_TrimStartAndEnd()
    {
        const string csv = "id,name\n1,  Ada  \n";
        var options = new CsvOptions
        {
            TrimOptions = CsvTrimOptions.Trim
        };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.TryReadRow(out var row));
        Assert.Equal("Ada", row.GetFieldString(1));
    }

    [Fact]
    public void DetectColumnCount_ThrowsInStrictMode()
    {
        const string csv = "a,b\n1,2\n3\n";
        var options = new CsvOptions
        {
            DetectColumnCount = true,
            ReadMode = CsvReadMode.Strict
        };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.Read());
        Assert.Throws<CsvException>(() => reader.Read());
    }

    [Fact]
    public void LenientMode_InvokesBadDataCallback()
    {
        const string csv = "a,b\n1,te\"st\n";
        var callbacks = 0;

        var options = new CsvOptions
        {
            ReadMode = CsvReadMode.Lenient,
            BadDataFound = _ => callbacks++
        };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.Read());
        Assert.Equal(1, callbacks);
    }

    [Fact]
    public void SpanAccess_ReturnsFieldSlices()
    {
        const string csv = "id,name\n1,Ada\n";

        using var reader = new CsvReader(new StringReader(csv));

        Assert.True(reader.Read());
        var span = reader.GetFieldSpan(1);
        Assert.True(span.SequenceEqual("Ada"));
    }

    [Fact]
    public void IgnoreBlankLines_SkipsEmptyRows()
    {
        const string csv = "id,name\n1,Ada\n\n2,Bob\n";
        var options = new CsvOptions { IgnoreBlankLines = true };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.Read());
        Assert.Equal("Ada", reader.GetField(1));
        Assert.True(reader.Read());
        Assert.Equal("Bob", reader.GetField(1));
        Assert.False(reader.Read());
    }

    [Fact]
    public void CultureAwareParsing_ParsesDecimal()
    {
        const string csv = "amount;date\n12,5;31/12/2025\n";
        var options = new CsvOptions
        {
            Delimiter = ';',
            CultureInfo = CultureInfo.GetCultureInfo("fr-FR")
        };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.Read());
        var row = reader.GetRecord<CultureRecord>();
        Assert.Equal(12.5m, row.Amount);
        Assert.Equal(new DateOnly(2025, 12, 31), row.Date);
    }

    [Fact]
    public void MissingField_ThrowsInStrictMode()
    {
        const string csv = "id\n1\n";
        var options = new CsvOptions
        {
            ReadMode = CsvReadMode.Strict
        };

        using var reader = new CsvReader(new StringReader(csv), options);

        Assert.True(reader.Read());
        Assert.Throws<CsvException>(() => reader.GetRecord<RequiredColumnsRecord>());
    }

    private sealed class CultureRecord
    {
        public decimal Amount { get; set; }

        public DateOnly Date { get; set; }
    }

    private sealed class RequiredColumnsRecord
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}