# Performance Design Decisions

This document explains the core design decisions behind CsvToolkit's performance profile and why they improve throughput and/or memory usage.

## Goals

- Stream data row-by-row without loading full files into memory.
- Keep allocations predictable and low on hot paths.
- Support both synchronous and asynchronous APIs without penalizing sync scenarios.
- Preserve correctness for quotes, escapes, custom delimiters, and culture-aware conversion.

## Hot Path Overview

The main read path is:

1. `CsvReader` orchestrates row iteration and mapping.
2. `CsvParser` scans characters and tokenizes fields.
3. `CsvRowBuffer` stores row chars/tokens in pooled buffers.
4. `CsvValueConverter` parses field spans into typed values.

Most CPU time is spent in parser character loops and conversion logic. Most allocations are from materializing strings/dictionaries and user-facing object creation.

## Key Decisions

### 1. Separate sync and async parser loops

Code:

- `src/CsvToolkit/Internal/CsvParser.cs`
- `TryReadRowCore()` (sync)
- `TryReadRowCoreAsync(...)` (async)

Why it is faster:

- Avoids sync-over-async overhead in the synchronous read path.
- Removes per-iteration branching between sync and async execution modes.
- Keeps the JIT-optimized sync loop small and predictable.

Trade-off:

- Duplicate logic between sync and async loops increases maintenance effort.

### 2. Hoist parser options into local variables per row

Code:

- `src/CsvToolkit/Internal/CsvParser.cs`

Why it is faster:

- Reduces repeated property access inside character-by-character loops.
- Cuts repeated bitwise checks for trim behavior and escape mode.

### 3. Keep row data in pooled buffers and defer string creation

Code:

- `src/CsvToolkit/Internal/PooledCharBuffer.cs`
- `src/CsvToolkit/Internal/PooledList.cs`
- `src/CsvToolkit/Internal/CsvRowBuffer.cs`
- `src/CsvToolkit/CsvRow.cs`

Why it is faster:

- Reuses arrays via `ArrayPool<T>` to reduce GC pressure.
- Uses token metadata (`start`, `length`) instead of copying field substrings.
- Allows span/memory access APIs (`GetFieldSpan`, `GetFieldMemory`) with no string allocation.

### 4. Cache built-in conversion metadata

Code:

- `src/CsvToolkit/TypeConversion/CsvValueConverter.cs`

Why it is faster:

- Caches resolved nullable/effective type information for repeated record parsing.
- Replaces repeated type inspection with a compact enum-based switch.
- Avoids temporary string allocation for `"1"`/`"0"` bool conversion by using single-char checks.

Trade-off:

- Small static cache memory overhead for seen types.

### 5. Cache generated `ColumnN` names

Code:

- `src/CsvToolkit/CsvReader.cs` (`GetGeneratedColumnName`)

Why it is faster:

- Avoids repeated interpolated string allocations when dictionary keys must be synthesized for missing headers.

### 6. Apply targeted inlining on tiny hot methods

Code:

- `src/CsvToolkit/Internal/CsvParser.cs` (`PushBack`, `ReadChar`)
- `src/CsvToolkit/CsvRow.cs` (`GetFieldSpan`, `GetFieldMemory`, `EnsureIndex`)

Why it is faster:

- Reduces call overhead for methods executed at very high frequency in tight loops.
- Keeps inlining focused to avoid unnecessary code-size growth.

## Measured Outcome (Latest Benchmark Run)

Source artifacts:

- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Benchmarks.CsvReadWriteBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/CsvToolkit.Benchmarks.CsvReadWriteBenchmarks-report.csv`

Run metadata:

- Date: `2026-02-10`
- Machine: `Apple M3 Pro`
- Runtime: `.NET 10.0.0`

Selected results (`RowCount=100000`):

- `CsvToolkit_WriteTyped_Stream`: `20.76 ms`, `25.97 MB`
- `CsvToolkit_ReadDictionary_Stream`: `26.78 ms`, `52.64 MB`
- `CsvToolkit_ReadTyped_SemicolonHighQuote`: `48.81 ms`, `19.2 MB`
- `CsvToolkit_ReadTyped_Stream`: `49.90 ms`, `19.2 MB`

## Design Principles Going Forward

- Optimize where benchmarks show sustained hot spots, not by default everywhere.
- Prefer low-allocation APIs (`ReadOnlySpan<char>`, `ReadOnlyMemory<char>`) at boundaries.
- Keep optimizations measurable and reversible with benchmark evidence.
