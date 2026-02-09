# CsvToolkit

`CsvToolkit` is a high-performance CSV library for `net10.0` focused on streaming and low allocations with `Span<T>`, `Memory<T>`, and `ArrayPool<T>`.

## Public API Proposal

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
  - Attributes: `[CsvColumn]`, `[CsvIndex]`, `[CsvIgnore]`
  - Fluent mapping: `CsvMapRegistry.Register<T>(...)`
- Type conversion:
  - primitives, enums, nullable, `DateTime`, `DateOnly`, `TimeOnly`, `Guid`
  - culture-aware parsing/formatting
  - custom converters (`ICsvTypeConverter<T>`)
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

## Benchmarks

Benchmarks compare `CsvToolkit` with `CsvHelper` for:

- Typed read (`100k` rows)
- Dictionary/dynamic read
- Typed write
- Semicolon + high quoting parse

Run all benchmarks non-interactively:

```bash
dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*"
```

Run one benchmark (faster while iterating):

```bash
dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*CsvReadWriteBenchmarks.CsvToolkit_ReadTyped_Stream*"
```

Run from IDE:

- Project: `benchmarks/CsvToolkit.Benchmarks`
- Configuration: `Release`
- Program arguments: `--filter "*"`

If you run without `--filter`, BenchmarkDotNet enters interactive selection mode and waits for input.

Benchmark dataset generation is deterministic (`Random` seed-based) inside benchmark setup.

### Latest Results

Run date: `2026-02-09`  
Machine: `Apple M3 Pro`  
Runtime: `.NET 10.0.0`  
Command: `dotnet run -c Release --project benchmarks/CsvToolkit.Benchmarks -- --filter "*CsvReadWriteBenchmarks*"`

| Method                                  | RowCount | Mean     | Error    | StdDev   | Ratio | Gen0      | Gen1     | Gen2     | Allocated | Alloc Ratio |
|---------------------------------------- |--------- |---------:|---------:|---------:|------:|----------:|---------:|---------:|----------:|------------:|
| CsvToolkit_WriteTyped_Stream            | 100000   | 21.29 ms | 0.034 ms | 0.032 ms |  0.40 | 1593.7500 | 406.2500 | 343.7500 |  25.97 MB |        1.35 |
| CsvHelper_WriteTyped_Stream             | 100000   | 24.35 ms | 0.078 ms | 0.069 ms |  0.46 | 3281.2500 | 656.2500 | 343.7500 |  39.41 MB |        2.05 |
| CsvToolkit_ReadDictionary_Stream        | 100000   | 30.60 ms | 0.133 ms | 0.111 ms |  0.58 | 6593.7500 |        - |        - |  52.64 MB |        2.74 |
| CsvHelper_ReadDynamic_Stream            | 100000   | 44.74 ms | 0.060 ms | 0.053 ms |  0.84 | 9916.6667 | 250.0000 |        - |  79.41 MB |        4.14 |
| CsvHelper_ReadTyped_Stream              | 100000   | 46.73 ms | 0.129 ms | 0.108 ms |  0.88 | 4545.4545 | 181.8182 |        - |  36.72 MB |        1.91 |
| CsvHelper_ReadTyped_SemicolonHighQuote  | 100000   | 49.82 ms | 0.165 ms | 0.138 ms |  0.94 | 4600.0000 | 200.0000 |        - |  36.72 MB |        1.91 |
| CsvToolkit_ReadTyped_Stream             | 100000   | 53.07 ms | 0.367 ms | 0.307 ms |  1.00 | 2400.0000 |        - |        - |   19.2 MB |        1.00 |
| CsvToolkit_ReadTyped_SemicolonHighQuote | 100000   | 57.39 ms | 0.140 ms | 0.131 ms |  1.08 | 2333.3333 |        - |        - |   19.2 MB |        1.00 |

Raw benchmark artifacts:
- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Benchmarks.CsvReadWriteBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Benchmarks.CsvReadWriteBenchmarks-report.csv`
