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

Full paired comparison (all like-for-like scenarios in this run):

| Scenario | RowCount | CsvToolkit.Core (Mean / Alloc) | CsvHelper (Mean / Alloc) | Time Winner | Allocation Winner |
|--------- |---------:|-------------------------------:|--------------------------:|------------:|------------------:|
| ReadTyped | 10,000 | 4.266 ms / 0.99 MB | 4.647 ms / 3.76 MB | CsvToolkit (`1.09x`) | CsvToolkit (`3.79x` lower) |
| WriteTyped | 10,000 | 2.655 ms / 3.33 MB | 2.840 ms / 3.45 MB | CsvToolkit (`1.07x`) | CsvToolkit (`1.04x` lower) |
| ReadTyped_SemicolonHighQuote | 10,000 | 4.623 ms / 0.99 MB | 5.129 ms / 3.76 MB | CsvToolkit (`1.11x`) | CsvToolkit (`3.79x` lower) |
| ReadTyped_WithConverterOptions | 10,000 | 2.784 ms / 1.33 MB | 3.024 ms / 2.67 MB | CsvToolkit (`1.09x`) | CsvToolkit (`2.01x` lower) |
| WriteTyped_WithConverterOptions | 10,000 | 2.188 ms / 2.39 MB | 2.346 ms / 2.46 MB | CsvToolkit (`1.07x`) | CsvToolkit (`1.03x` lower) |
| ReadTyped_DuplicateHeader_NameIndex | 10,000 | 1.342 ms / 1.17 MB | 1.597 ms / 1.78 MB | CsvToolkit (`1.19x`) | CsvToolkit (`1.51x` lower) |
| ReadTyped | 100,000 | 41.431 ms / 9.30 MB | 45.086 ms / 36.72 MB | CsvToolkit (`1.09x`) | CsvToolkit (`3.95x` lower) |
| WriteTyped | 100,000 | 23.988 ms / 38.86 MB | 22.670 ms / 39.40 MB | CsvHelper (`1.06x`) | CsvToolkit (`1.01x` lower) |
| ReadTyped_SemicolonHighQuote | 100,000 | 40.004 ms / 9.30 MB | 48.538 ms / 36.72 MB | CsvToolkit (`1.21x`) | CsvToolkit (`3.95x` lower) |
| ReadTyped_WithConverterOptions | 100,000 | 26.408 ms / 12.80 MB | 27.290 ms / 25.81 MB | CsvToolkit (`1.03x`) | CsvToolkit (`2.02x` lower) |
| WriteTyped_WithConverterOptions | 100,000 | 19.295 ms / 26.52 MB | 19.087 ms / 26.60 MB | CsvHelper (`1.01x`) | CsvToolkit (`~0.3%` lower) |
| ReadTyped_DuplicateHeader_NameIndex | 100,000 | 13.088 ms / 12.23 MB | 13.932 ms / 17.64 MB | CsvToolkit (`1.06x`) | CsvToolkit (`1.44x` lower) |

Additional dynamic-style scenario (`ReadDictionary` vs `ReadDynamic`, not strict API-equivalent):

| Scenario | RowCount | CsvToolkit.Core (Mean / Alloc) | CsvHelper (Mean / Alloc) | Time | Allocation |
|--------- |---------:|-------------------------------:|--------------------------:|-----:|-----------:|
| ReadDictionary vs ReadDynamic | 10,000 | 2.583 ms / 5.26 MB | 4.257 ms / 8.00 MB | CsvToolkit (`1.65x` faster) | CsvToolkit (`1.52x` lower) |
| ReadDictionary vs ReadDynamic | 100,000 | 27.228 ms / 52.64 MB | 42.923 ms / 79.41 MB | CsvToolkit (`1.58x` faster) | CsvToolkit (`1.51x` lower) |

Observed trend from this run:
- At `10k` rows, `CsvToolkit.Core` is faster and lower-allocation in all `6/6` paired scenarios.
- At `100k` rows, `CsvToolkit.Core` is faster in `4/6` paired scenarios and lower-allocation in `6/6` paired scenarios.
- Typed read paths keep the largest memory advantage (`~2x` to `~4x` lower allocation).

Benchmark run time:
- Benchmark execution: `00:08:46` (`526.66 sec`)
- Global total: `00:09:04` (`544.68 sec`)

Benchmark artifacts:
- Tracked snapshot (Markdown): `docs/benchmarks/CsvReadWriteBenchmarks-2026-03-04.md`
- Tracked snapshot (CSV): `docs/benchmarks/CsvReadWriteBenchmarks-2026-03-04.csv`
- Generated local output (gitignored): `BenchmarkDotNet.Artifacts/results/`
