# CsvToolkit.Core

<p align="center">
  <img src="docs/assets/csvtoolkit-logo-dark.svg" alt="CsvToolkit.Core logo" width="760" />
</p>

[![NuGet version](https://img.shields.io/nuget/vpre/CsvToolkit.Core.svg)](https://www.nuget.org/packages/CsvToolkit.Core)
[![publish](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish.yml/badge.svg)](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish.yml)
[![publish-beta](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish-beta.yml/badge.svg)](https://github.com/gabisonia/CsvToolkitCore/actions/workflows/publish-beta.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

`CsvToolkit.Core` is a high-performance CSV library targeting `netstandard2.1`, focused on streaming and low allocations with `Span<T>`, `Memory<T>`, and `ArrayPool<T>`.

## Benchmark Note

[`Sep`](https://github.com/nietras/Sep) is still faster in the latest benchmark runs, but the read-path gap narrowed materially after the latest parser optimization. In the focused `100k`-row rerun, `CsvToolkit.Core` typed read improved from about `42.5 ms` to `31.4 ms`, while semicolon/high-quote read improved from about `40.8 ms` to `33.3 ms`. If your main KPI is raw CSV throughput above all else, [`Sep`](https://github.com/nietras/Sep) remains the speed ceiling; `CsvToolkit.Core` is closing the gap while keeping a higher-level typed API and broader feature coverage.

## NuGet

Package name on NuGet.org: `CsvToolkit.Core`

```bash
dotnet add package CsvToolkit.Core
```

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE).

## Public API Overview

```csharp
var options = new CsvOptions
{
    DelimiterString = ",", // supports multi-character delimiters too
    DetectDelimiter = false,
    DelimiterCandidates = new[] { ",", ";", "\t", "|" },
    HasHeader = true,
    Quote = '"',
    Escape = '"',
    TrimOptions = CsvTrimOptions.Trim,
    DetectColumnCount = true,
    ReadMode = CsvReadMode.Strict,
    CultureInfo = CultureInfo.InvariantCulture,
    PrepareHeaderForMatch = static (header, _) => header.Trim().ToLowerInvariant(),
    SanitizeForInjection = true
};

using var reader = new CsvReader(streamOrTextReader, options);
using var writer = new CsvWriter(streamOrTextWriter, options);

// Row/field iteration
while (reader.TryReadRow(out _))
{
    int id = reader.GetField<int>("id");
    ReadOnlySpan<char> name = reader.GetFieldSpan(1);
    string materialized = reader.GetField("email");
}

// Dictionary / dynamic
if (reader.TryReadDictionary(out var dict)) { /* header -> value */ }
if (reader.TryReadDynamic(out dynamic dyn)) { /* ExpandoObject */ }

// Strongly typed POCO
while (reader.TryReadRecord<MyRow>(out var record)) { }
writer.WriteHeader<MyRow>();
writer.WriteRecord(new MyRow());
var records = new List<MyRow> { new MyRow() };
writer.WriteRecords(records, writeHeader: false);

// Async
while (await reader.ReadAsync()) { var current = reader.GetRecord<MyRow>(); }
await reader.ReadRecordAsync<MyRow>();
await foreach (var row in reader.GetRecordsAsync<MyRow>()) { }
await writer.WriteRecordAsync(new MyRow());
await writer.WriteRecordsAsync(records, writeHeader: true);

// ADO.NET adapter
using var dataReader = reader.AsDataReader();
```

## Features

- Read from `TextReader` and UTF-8 `Stream`
- Write to `TextWriter` and UTF-8 `Stream`
- Streaming row-by-row parser (no full-file load)
- Quoted fields, escaped quotes, delimiters inside quotes, CRLF/LF handling
- Header support, delimiter/quote/escape/newline configuration
- Multi-character delimiters via `DelimiterString`
- Optional delimiter auto-detection via `DetectDelimiter` + `DelimiterCandidates`
- Trim options and strict/lenient error handling
- Error and validation callbacks: `BadDataFound`, `MissingFieldFound`, `HeaderValidated`, `ReadingExceptionOccurred`
- Header normalization callback: `PrepareHeaderForMatch`
- Field access as `ReadOnlySpan<char>` / `ReadOnlyMemory<char>`
- Typed field access helpers: `GetField<T>(index)` / `GetField<T>(name, nameIndex)`
- Reader APIs: `TryReadRow`, `ReadAsync`, `TryReadDictionary`, `ReadDictionaryAsync`, `TryReadDynamic`
- Record APIs: `TryReadRecord<T>`, `ReadRecordAsync<T>`, `GetRecords<T>`, `GetRecordsAsync<T>`
- `CsvDataReader` adapter via `AsDataReader()`
- POCO mapping with:
  - Attributes: `[CsvColumn]`, `[CsvIndex]`, `[CsvIgnore]`, `[CsvNameIndex]`, `[CsvOptional]`, `[CsvDefault]`, `[CsvConstant]`, `[CsvValidate]`
  - Converter option attributes: `[CsvNullValues]`, `[CsvTrueValues]`, `[CsvFalseValues]`, `[CsvFormats]`, `[CsvNumberStyles]`, `[CsvDateTimeStyles]`, `[CsvCulture]`
  - Fluent mapping: `CsvMapRegistry.Register<T>(...)` with `Optional`, `Default`, `Constant`, `Validate`, `NameIndex`, member converter options
  - Constructor-based record materialization (immutable / constructor-only models)
- Type conversion:
  - primitives, enums, nullable, `DateTime`, `DateOnly`, `TimeOnly`, `Guid`
  - culture-aware parsing/formatting
  - custom converters (`ICsvTypeConverter<T>`)
  - global converter options per type via `CsvOptions.ConverterOptions`
- Writer bulk APIs: `WriteRecords(...)`, `WriteRecordsAsync(...)`
- CSV injection sanitization for spreadsheet-safe output (`SanitizeForInjection`)

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

Benchmarks compare `CsvToolkit.Core` with `CsvHelper` and `Sep` for:

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

Run date: `2026-03-07`  
Machine: `Apple M3 Pro` (`11` logical / `11` physical cores)  
OS: `macOS Tahoe 26.3 (25D125) [Darwin 25.3.0]`  
Runtime: `.NET 10.0.0`  
Command: `dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*CsvReadWriteBenchmarks*"`

### Focused Read Rerun After Parser Optimization

Run date: `2026-03-07`  
Machine: `Apple M3 Pro` (`11` logical / `11` physical cores)  
OS: `macOS Tahoe 26.3 (25D125) [Darwin 25.3.0]`  
Runtime: `.NET 10.0.0`  
Command:

```bash
dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter \
  "*CsvReadWriteBenchmarks.CsvToolkitCore_ReadTyped_Stream*" \
  "*CsvReadWriteBenchmarks.Sep_ReadTyped_Stream*" \
  "*CsvReadWriteBenchmarks.CsvToolkitCore_ReadTyped_SemicolonHighQuote*" \
  "*CsvReadWriteBenchmarks.Sep_ReadTyped_SemicolonHighQuote*"
```

This rerun reflects the latest single-character delimiter parser fast path in `CsvParser`.

| Scenario | RowCount | CsvToolkit.Core (Mean / Alloc) | Sep (Mean / Alloc) | Time Winner | Allocation Winner |
|--------- |---------:|-------------------------------:|-------------------:|------------:|------------------:|
| ReadTyped | 10,000 | 3.580 ms / 0.99 MB | 2.080 ms / 0.92 MB | Sep (`1.72x`) | Sep (`1.08x` lower) |
| ReadTyped_SemicolonHighQuote | 10,000 | 3.640 ms / 0.99 MB | 2.284 ms / 0.92 MB | Sep (`1.59x`) | Sep (`1.08x` lower) |
| ReadTyped | 100,000 | 31.366 ms / 9.30 MB | 21.489 ms / 9.23 MB | Sep (`1.46x`) | Sep (`1.01x` lower) |
| ReadTyped_SemicolonHighQuote | 100,000 | 33.270 ms / 9.30 MB | 23.560 ms / 9.23 MB | Sep (`1.41x`) | Sep (`1.01x` lower) |

Observed change from the previous tracked `2026-03-07` full benchmark snapshot:
- `CsvToolkit.Core` `ReadTyped` improved by about `22.6%` at `10k` rows and about `26.1%` at `100k` rows.
- `CsvToolkit.Core` `ReadTyped_SemicolonHighQuote` improved by about `15.5%` at `10k` rows and about `18.5%` at `100k` rows.
- Read allocations stayed effectively flat, which indicates the improvement came from parser CPU work rather than allocation changes.

### Full Cross-Library Snapshot

The table below is the latest full benchmark run across all benchmark cases. Its read-path numbers predate the focused parser rerun above.

Common scenarios benchmarked across all three libraries:

| Scenario | RowCount | CsvToolkit.Core (Mean / Alloc) | CsvHelper (Mean / Alloc) | Sep (Mean / Alloc) | Time Winner | Allocation Winner |
|--------- |---------:|-------------------------------:|--------------------------:|-------------------:|------------:|------------------:|
| ReadTyped | 10,000 | 4.618 ms / 0.99 MB | 5.029 ms / 3.76 MB | 1.992 ms / 0.92 MB | Sep (`2.32x` vs CsvToolkit) | Sep (`1.08x` lower vs CsvToolkit) |
| WriteTyped | 10,000 | 2.780 ms / 3.33 MB | 2.932 ms / 3.45 MB | 1.161 ms / 2.01 MB | Sep (`2.39x`) | Sep (`1.66x` lower) |
| ReadTyped_SemicolonHighQuote | 10,000 | 4.309 ms / 0.99 MB | 5.472 ms / 3.76 MB | 2.271 ms / 0.92 MB | Sep (`1.90x`) | Sep (`1.08x` lower) |
| ReadTyped_WithConverterOptions | 10,000 | 2.963 ms / 1.33 MB | 3.170 ms / 2.67 MB | 1.084 ms / 0.39 MB | Sep (`2.73x`) | Sep (`3.44x` lower) |
| WriteTyped_WithConverterOptions | 10,000 | 2.269 ms / 2.39 MB | 2.402 ms / 2.46 MB | 0.730 ms / 0.70 MB | Sep (`3.11x`) | Sep (`3.42x` lower) |
| ReadTyped | 100,000 | 42.462 ms / 9.30 MB | 45.436 ms / 36.72 MB | 21.452 ms / 9.23 MB | Sep (`1.98x`) | Sep (`1.01x` lower) |
| WriteTyped | 100,000 | 26.160 ms / 38.86 MB | 23.771 ms / 39.40 MB | 11.981 ms / 16.01 MB | Sep (`2.18x`) | Sep (`2.43x` lower) |
| ReadTyped_SemicolonHighQuote | 100,000 | 40.836 ms / 9.30 MB | 50.807 ms / 36.72 MB | 24.277 ms / 9.23 MB | Sep (`1.68x`) | Sep (`1.01x` lower) |
| ReadTyped_WithConverterOptions | 100,000 | 26.681 ms / 12.80 MB | 28.106 ms / 25.81 MB | 11.279 ms / 3.82 MB | Sep (`2.37x`) | Sep (`3.35x` lower) |
| WriteTyped_WithConverterOptions | 100,000 | 19.152 ms / 26.52 MB | 20.332 ms / 26.60 MB | 7.526 ms / 9.93 MB | Sep (`2.54x`) | Sep (`2.67x` lower) |

Additional scenarios:

| Scenario | RowCount | CsvToolkit.Core (Mean / Alloc) | CsvHelper (Mean / Alloc) | Time Winner | Allocation Winner |
|--------- |---------:|-------------------------------:|--------------------------:|------------:|------------------:|
| ReadTyped_DuplicateHeader_NameIndex | 10,000 | 1.395 ms / 1.17 MB | 1.610 ms / 1.78 MB | CsvToolkit (`1.15x`) | CsvToolkit (`1.51x` lower) |
| ReadTyped_DuplicateHeader_NameIndex | 100,000 | 13.341 ms / 12.23 MB | 14.285 ms / 17.64 MB | CsvToolkit (`1.07x`) | CsvToolkit (`1.44x` lower) |
| ReadDictionary vs ReadDynamic | 10,000 | 2.599 ms / 5.26 MB | 4.307 ms / 8.00 MB | CsvToolkit (`1.66x`) | CsvToolkit (`1.52x` lower) |
| ReadDictionary vs ReadDynamic | 100,000 | 26.987 ms / 52.64 MB | 44.269 ms / 79.41 MB | CsvToolkit (`1.64x`) | CsvToolkit (`1.51x` lower) |

Observed trend from this run:
- Across the `10` common scenarios benchmarked for all three libraries, `Sep` is the fastest and lowest-allocation option in `10/10`.
- `CsvToolkit.Core` still beats `CsvHelper` in `8/10` of those common scenarios; `CsvHelper` only wins the two `100k` write cases, and narrowly.
- In the extra `DuplicateHeader_NameIndex` scenario, `CsvToolkit.Core` beats `CsvHelper` at both sizes.
- Important caveat: `Sep` is benchmarked here through explicit/manual column mapping and writing, while `CsvToolkit.Core` and `CsvHelper` use higher-level typed POCO APIs. Treat `Sep` as a low-level throughput ceiling, not a direct ergonomic equivalent.

Benchmark run time:
- Benchmark execution: `00:18:42` (`1122.27 sec`)
- Global total: `00:18:47` (`1127.27 sec`)

Benchmark artifacts:
- Tracked snapshot (Markdown): `docs/benchmarks/CsvReadWriteBenchmarks-2026-03-07.md`
- Tracked snapshot (CSV): `docs/benchmarks/CsvReadWriteBenchmarks-2026-03-07.csv`
- Generated local output: `BenchmarkDotNet.Artifacts/results/`
