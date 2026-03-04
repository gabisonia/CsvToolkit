# CsvToolkit.Core

<p align="center">
  <img src="docs/assets/csvtoolkit-logo-dark.svg" alt="CsvToolkit.Core logo" width="760" />
</p>

[![NuGet version](https://img.shields.io/nuget/vpre/CsvToolkit.Core.svg)](https://www.nuget.org/packages/CsvToolkit.Core)
[![publish](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish.yml/badge.svg)](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish.yml)
[![publish-beta](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish-beta.yml/badge.svg)](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish-beta.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

`CsvToolkit.Core` is a high-performance CSV library for `net10.0` focused on streaming and low allocations with `Span<T>`, `Memory<T>`, and `ArrayPool<T>`.

## NuGet

Package name on NuGet.org: `CsvToolkit.Core`

```bash
dotnet add package CsvToolkit.Core --prerelease
```

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE).

## Public API Overview

```csharp
var options = new CsvOptions
{
    Delimiter = ',',
    HasHeader = true,
    Quote = '"',
    Escape = '"',
    TrimOptions = CsvTrimOptions.Trim,
    DetectColumnCount = true,
    ReadMode = CsvReadMode.Strict,
    CultureInfo = CultureInfo.InvariantCulture
};

using var reader = new CsvReader(streamOrTextReader, options);
using var writer = new CsvWriter(streamOrTextWriter, options);

// Row/field iteration
while (reader.TryReadRow(out var row))
{
    ReadOnlySpan<char> field = row.GetFieldSpan(0);
    string materialized = row.GetFieldString(1);
}

// Dictionary / dynamic
if (reader.TryReadDictionary(out var dict)) { /* header -> value */ }
if (reader.TryReadDynamic(out dynamic dyn)) { /* ExpandoObject */ }

// Strongly typed POCO
while (reader.TryReadRecord<MyRow>(out var record)) { }
writer.WriteHeader<MyRow>();
writer.WriteRecord(new MyRow());

// Async
await reader.ReadAsync();
await writer.WriteRecordAsync(new MyRow());
```

## Features

- Read from `TextReader` and UTF-8 `Stream`
- Write to `TextWriter` and UTF-8 `Stream`
- Streaming row-by-row parser (no full-file load)
- Quoted fields, escaped quotes, delimiters inside quotes, CRLF/LF handling
- Header support, delimiter/quote/escape/newline configuration
- Trim options and strict/lenient error handling with callback context
- Field access as `ReadOnlySpan<char>` / `ReadOnlyMemory<char>`
- POCO mapping with:
  - Attributes: `[CsvColumn]`, `[CsvIndex]`, `[CsvIgnore]`, `[CsvNameIndex]`, `[CsvOptional]`, `[CsvDefault]`, `[CsvConstant]`, `[CsvValidate]`
  - Converter option attributes: `[CsvNullValues]`, `[CsvTrueValues]`, `[CsvFalseValues]`, `[CsvFormats]`, `[CsvNumberStyles]`, `[CsvDateTimeStyles]`, `[CsvCulture]`
  - Fluent mapping: `CsvMapRegistry.Register<T>(...)` with `Optional`, `Default`, `Constant`, `Validate`, `NameIndex`, member converter options
- Type conversion:
  - primitives, enums, nullable, `DateTime`, `DateOnly`, `TimeOnly`, `Guid`
  - culture-aware parsing/formatting
  - custom converters (`ICsvTypeConverter<T>`)
  - global converter options per type via `CsvOptions.ConverterOptions`
- Async read/write entry points

## Examples

### Read POCOs

```csharp
using var reader = new CsvReader(new StreamReader("people.csv"));
while (reader.TryReadRecord<Person>(out var person))
{
    Console.WriteLine(person.Name);
}
```

### Write POCOs

```csharp
using var writer = new CsvWriter(File.Create("people.csv"), new CsvOptions { NewLine = "\n" });
writer.WriteHeader<Person>();
foreach (var person in people)
{
    writer.WriteRecord(person);
}
```

### Read Without String Allocations

```csharp
while (reader.TryReadRow(out var row))
{
    ReadOnlySpan<char> id = row.GetFieldSpan(0);
    // parse directly from span
}
```

### Fluent Mapping

```csharp
var maps = new CsvMapRegistry();
maps.Register<Person>(map =>
{
    map.Map(x => x.Id).Name("person_id");
    map.Map(x => x.Name).Name("full_name");
});
```

### Attribute Mapping and Converter Options

```csharp
public sealed class PersonRow
{
    [CsvColumn("name"), CsvNameIndex(0)]
    public string FirstName { get; set; } = string.Empty;

    [CsvColumn("name"), CsvNameIndex(1)]
    public string LastName { get; set; } = string.Empty;

    [CsvOptional, CsvDefault(18)]
    public int Age { get; set; }

    [CsvConstant("US")]
    public string Country { get; set; } = string.Empty;

    [CsvValidate(nameof(IsValidAge), Message = "Age must be >= 0.")]
    public int CheckedAge { get; set; }

    [CsvTrueValues("Y"), CsvFalseValues("N")]
    public bool Active { get; set; }

    [CsvFormats("dd-MM-yyyy"), CsvCulture("en-GB")]
    public DateTime CreatedAt { get; set; }

    [CsvNullValues("NULL")]
    public decimal? Score { get; set; }

    private static bool IsValidAge(int value) => value >= 0;
}
```

### Converter Option Precedence

For read/write conversion options, resolution order is:

1. Member-level fluent options (`map.Map(...).TrueValues(...).Formats(...)`)
2. Member-level attribute options (`[CsvTrueValues]`, `[CsvFormats]`, etc.)
3. Global type options (`options.ConverterOptions.Configure<T>(...)`)
4. Built-in defaults

Notes:
- Fluent member options override attribute options for the same member.
- Attribute/member options are isolated to that mapped member and do not affect other properties of the same type.
- `ConverterOptions` precedence applies to both `CsvReader` and `CsvWriter`.

## Benchmarks

Benchmarks compare `CsvToolkit.Core` with `CsvHelper` for:

- Typed read/write (default mapping)
- Typed read/write with converter options
- Typed read with duplicate headers (`NameIndex`)
- Dictionary/dynamic read
- Semicolon + high quoting parse
- Dataset sizes: `10k` and `100k` rows

### Technical Docs

If you want deeper implementation details and rationale, start here:

- [Technical Architecture](docs/technical-architecture.md): components, data flow, and technologies used.
- [Performance Design Decisions](docs/performance-design-decisions.md): why hot paths are fast and the tradeoffs behind each optimization.

Run all benchmarks non-interactively:

```bash
dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*"
```

Run one benchmark (faster while iterating):

```bash
dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*CsvReadWriteBenchmarks.CsvToolkitCore_ReadTyped_Stream*"
```

Run from IDE:

- Project: `benchmarks/CsvToolkit.Benchmarks`
- Configuration: `Release`
- Program arguments: `--filter "*"`

If you run without `--filter`, BenchmarkDotNet enters interactive selection mode and waits for input.

Benchmark dataset generation is deterministic (`Random` seed-based) inside benchmark setup.

### Latest Results

Run date: `2026-03-04`  
Machine: `Apple M3 Pro` (`11` logical / `11` physical cores)  
OS: `macOS Tahoe 26.3 (Darwin 25.3.0)`  
Runtime: `.NET 10.0.0`  
Command: `dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*CsvReadWriteBenchmarks*"`

Top-line results (`RowCount = 100000`):

| Method                                                    | Mean      | Allocated |
|---------------------------------------------------------- |----------:|----------:|
| CsvHelper_ReadTyped_DuplicateHeader_NameIndex_Stream      | 14.915 ms |  17.64 MB |
| CsvHelper_WriteTyped_WithConverterOptions_Stream          | 20.558 ms |   26.6 MB |
| CsvToolkitCore_ReadTyped_DuplicateHeader_NameIndex_Stream | 20.782 ms |   14.5 MB |
| CsvHelper_WriteTyped_Stream                               | 24.595 ms |   39.4 MB |
| CsvToolkitCore_ReadDictionary_Stream                      | 27.158 ms |  52.64 MB |
| CsvToolkitCore_WriteTyped_Stream                          | 27.503 ms |  38.86 MB |
| CsvToolkitCore_WriteTyped_WithConverterOptions_Stream     | 27.535 ms |  29.27 MB |
| CsvHelper_ReadTyped_WithConverterOptions_Stream           | 27.923 ms |  25.81 MB |
| CsvToolkitCore_ReadTyped_WithConverterOptions_Stream      | 39.036 ms |  15.54 MB |
| CsvHelper_ReadDynamic_Stream                              | 45.534 ms |  79.41 MB |
| CsvHelper_ReadTyped_Stream                                | 46.115 ms |  36.72 MB |
| CsvHelper_ReadTyped_SemicolonHighQuote                    | 50.544 ms |  36.72 MB |
| CsvToolkitCore_ReadTyped_SemicolonHighQuote               | 54.435 ms |   19.2 MB |
| CsvToolkitCore_ReadTyped_Stream                           | 56.497 ms |   19.2 MB |

Observed trend from this run:
- `CsvToolkit.Core` generally allocates less memory in typed read scenarios.
- `CsvHelper` currently leads on typed throughput in most scenarios.
- Converter-options and duplicate-header cases show the same tradeoff: lower allocation in `CsvToolkit.Core`, faster execution in `CsvHelper`.

Benchmark run time:
- Benchmark execution: `00:11:01` (`661.2 sec`)
- Global total: `00:11:06` (`666.61 sec`)

Raw benchmark artifacts:
- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Core.Benchmarks.CsvReadWriteBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Core.Benchmarks.CsvReadWriteBenchmarks-report.csv`
