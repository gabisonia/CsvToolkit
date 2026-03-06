```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.3 (25D125) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), Arm64 RyuJIT armv8.0-a


```
| Method                                                    | RowCount | Mean        | Error     | StdDev      | Median      | Ratio | RatioSD | Gen0      | Gen1     | Gen2     | Allocated   | Alloc Ratio |
|---------------------------------------------------------- |--------- |------------:|----------:|------------:|------------:|------:|--------:|----------:|---------:|---------:|------------:|------------:|
| Sep_WriteTyped_WithConverterOptions_Stream                | 10000    |    729.7 μs |  13.19 μs |    27.23 μs |    719.8 μs |  0.16 |    0.01 |  119.1406 |  83.0078 |  79.1016 |   715.05 KB |        0.70 |
| Sep_ReadTyped_WithConverterOptions_Stream                 | 10000    |  1,084.0 μs |   9.20 μs |     7.69 μs |  1,079.4 μs |  0.24 |    0.01 |   46.8750 |        - |        - |   395.09 KB |        0.39 |
| Sep_WriteTyped_Stream                                     | 10000    |  1,161.4 μs |  23.12 μs |    22.71 μs |  1,155.3 μs |  0.25 |    0.01 |  347.6563 | 332.0313 | 332.0313 |  2054.51 KB |        2.02 |
| CsvToolkitCore_ReadTyped_DuplicateHeader_NameIndex_Stream | 10000    |  1,395.0 μs |  27.59 μs |    75.07 μs |  1,372.3 μs |  0.30 |    0.02 |  146.4844 |  11.7188 |        - |  1201.39 KB |        1.18 |
| CsvHelper_ReadTyped_DuplicateHeader_NameIndex_Stream      | 10000    |  1,610.3 μs |   4.41 μs |     3.68 μs |  1,609.7 μs |  0.35 |    0.01 |  222.6563 |  50.7813 |        - |  1819.44 KB |        1.79 |
| Sep_ReadTyped_Stream                                      | 10000    |  1,991.8 μs |   8.90 μs |     7.89 μs |  1,988.8 μs |  0.43 |    0.01 |  113.2813 |        - |        - |   941.98 KB |        0.93 |
| CsvToolkitCore_WriteTyped_WithConverterOptions_Stream     | 10000    |  2,268.7 μs |  34.82 μs |    30.86 μs |  2,269.6 μs |  0.49 |    0.02 |  320.3125 | 117.1875 |  70.3125 |  2443.51 KB |        2.41 |
| Sep_ReadTyped_SemicolonHighQuote                          | 10000    |  2,271.2 μs |   6.45 μs |     5.72 μs |  2,272.8 μs |  0.49 |    0.02 |  113.2813 |        - |        - |   941.98 KB |        0.93 |
| CsvHelper_WriteTyped_WithConverterOptions_Stream          | 10000    |  2,401.6 μs |  47.15 μs |    54.30 μs |  2,371.4 μs |  0.52 |    0.02 |  320.3125 | 148.4375 |  62.5000 |  2522.05 KB |        2.49 |
| CsvToolkitCore_ReadDictionary_Stream                      | 10000    |  2,598.6 μs |  17.38 μs |    16.26 μs |  2,593.8 μs |  0.56 |    0.02 |  656.2500 |  11.7188 |        - |  5389.43 KB |        5.31 |
| CsvToolkitCore_WriteTyped_Stream                          | 10000    |  2,780.1 μs |  34.59 μs |    28.88 μs |  2,787.1 μs |  0.60 |    0.02 |  464.8438 | 218.7500 | 160.1563 |   3406.7 KB |        3.36 |
| CsvHelper_WriteTyped_Stream                               | 10000    |  2,931.7 μs |  56.99 μs |    81.74 μs |  2,917.5 μs |  0.64 |    0.03 |  453.1250 | 265.6250 | 140.6250 |  3534.35 KB |        3.48 |
| CsvToolkitCore_ReadTyped_WithConverterOptions_Stream      | 10000    |  2,963.1 μs |  58.43 μs |   111.17 μs |  2,924.5 μs |  0.64 |    0.03 |  164.0625 |  31.2500 |        - |  1359.72 KB |        1.34 |
| CsvHelper_ReadTyped_WithConverterOptions_Stream           | 10000    |  3,169.8 μs |  62.68 μs |   128.03 μs |  3,124.6 μs |  0.69 |    0.04 |  332.0313 |  82.0313 |        - |  2735.88 KB |        2.70 |
| CsvHelper_ReadDynamic_Stream                              | 10000    |  4,306.9 μs |  49.15 μs |    41.04 μs |  4,286.0 μs |  0.93 |    0.03 | 1000.0000 | 195.3125 |        - |  8190.13 KB |        8.07 |
| CsvToolkitCore_ReadTyped_SemicolonHighQuote               | 10000    |  4,308.7 μs |  58.97 μs |    52.28 μs |  4,293.6 μs |  0.93 |    0.03 |  109.3750 |  31.2500 |        - |  1014.67 KB |        1.00 |
| CsvToolkitCore_ReadTyped_Stream                           | 10000    |  4,617.5 μs |  91.81 μs |   163.19 μs |  4,526.7 μs |  1.00 |    0.05 |  117.1875 |  39.0625 |        - |  1014.79 KB |        1.00 |
| CsvHelper_ReadTyped_Stream                                | 10000    |  5,028.7 μs |  86.69 μs |   186.60 μs |  4,954.1 μs |  1.09 |    0.05 |  468.7500 | 117.1875 |        - |  3849.14 KB |        3.79 |
| CsvHelper_ReadTyped_SemicolonHighQuote                    | 10000    |  5,472.4 μs | 109.44 μs |   223.56 μs |  5,412.9 μs |  1.19 |    0.06 |  468.7500 | 109.3750 |        - |  3849.14 KB |        3.79 |
|                                                           |          |             |           |             |             |       |         |           |          |          |             |             |
| Sep_WriteTyped_WithConverterOptions_Stream                | 100000   |  7,525.7 μs | 132.69 μs |   181.63 μs |  7,476.1 μs |  0.18 |    0.00 |  312.5000 |  70.3125 |  62.5000 |    10167 KB |        1.07 |
| Sep_ReadTyped_WithConverterOptions_Stream                 | 100000   | 11,278.8 μs | 223.06 μs |   407.89 μs | 11,099.6 μs |  0.27 |    0.01 |  468.7500 |        - |        - |  3910.72 KB |        0.41 |
| Sep_WriteTyped_Stream                                     | 100000   | 11,980.6 μs | 237.98 μs |   410.50 μs | 11,868.7 μs |  0.28 |    0.01 |  171.8750 | 156.2500 | 156.2500 |  16390.4 KB |        1.72 |
| CsvToolkitCore_ReadTyped_DuplicateHeader_NameIndex_Stream | 100000   | 13,340.5 μs | 260.04 μs |   319.36 μs | 13,277.0 μs |  0.31 |    0.01 | 1531.2500 |  15.6250 |        - | 12522.83 KB |        1.31 |
| CsvHelper_ReadTyped_DuplicateHeader_NameIndex_Stream      | 100000   | 14,284.6 μs | 251.24 μs |   335.40 μs | 14,211.9 μs |  0.34 |    0.01 | 2203.1250 | 250.0000 |        - | 18063.88 KB |        1.90 |
| CsvToolkitCore_WriteTyped_WithConverterOptions_Stream     | 100000   | 19,151.6 μs |  81.54 μs |    76.27 μs | 19,151.1 μs |  0.45 |    0.01 | 2375.0000 |  93.7500 |  62.5000 | 27155.72 KB |        2.85 |
| CsvHelper_WriteTyped_WithConverterOptions_Stream          | 100000   | 20,331.8 μs | 406.30 μs | 1,019.33 μs | 20,114.1 μs |  0.48 |    0.02 | 2406.2500 | 187.5000 |  62.5000 | 27235.44 KB |        2.86 |
| Sep_ReadTyped_Stream                                      | 100000   | 21,451.5 μs | 166.90 μs |   163.92 μs | 21,389.2 μs |  0.51 |    0.01 | 1156.2500 |        - |        - |  9450.38 KB |        0.99 |
| CsvHelper_WriteTyped_Stream                               | 100000   | 23,771.3 μs | 466.96 μs |   389.93 μs | 23,719.1 μs |  0.56 |    0.01 | 3093.7500 | 343.7500 | 156.2500 |  40348.3 KB |        4.24 |
| Sep_ReadTyped_SemicolonHighQuote                          | 100000   | 24,276.9 μs | 482.74 μs |   451.55 μs | 24,311.3 μs |  0.57 |    0.01 | 1156.2500 |        - |        - |  9450.38 KB |        0.99 |
| CsvToolkitCore_WriteTyped_Stream                          | 100000   | 26,160.1 μs | 508.25 μs |   543.82 μs | 26,055.4 μs |  0.62 |    0.01 | 3031.2500 | 218.7500 | 156.2500 | 39792.65 KB |        4.18 |
| CsvToolkitCore_ReadTyped_WithConverterOptions_Stream      | 100000   | 26,680.5 μs | 530.00 μs |   413.79 μs | 26,562.3 μs |  0.63 |    0.01 | 1593.7500 |  31.2500 |        - | 13104.67 KB |        1.38 |
| CsvToolkitCore_ReadDictionary_Stream                      | 100000   | 26,987.4 μs | 195.13 μs |   152.34 μs | 26,959.6 μs |  0.64 |    0.01 | 6593.7500 |        - |        - | 53905.89 KB |        5.66 |
| CsvHelper_ReadTyped_WithConverterOptions_Stream           | 100000   | 28,106.0 μs | 533.06 μs |   654.64 μs | 27,973.4 μs |  0.66 |    0.02 | 3218.7500 | 250.0000 |        - | 26427.48 KB |        2.77 |
| CsvToolkitCore_ReadTyped_SemicolonHighQuote               | 100000   | 40,835.7 μs | 648.83 μs |   909.57 μs | 40,530.8 μs |  0.96 |    0.02 | 1153.8462 |  76.9231 |        - |  9526.68 KB |        1.00 |
| CsvToolkitCore_ReadTyped_Stream                           | 100000   | 42,461.8 μs | 534.48 μs |   524.93 μs | 42,365.2 μs |  1.00 |    0.02 | 1083.3333 |        - |        - |  9526.17 KB |        1.00 |
| CsvHelper_ReadDynamic_Stream                              | 100000   | 44,269.3 μs | 881.36 μs |   781.30 μs | 44,147.0 μs |  1.04 |    0.02 | 9916.6667 | 250.0000 |        - | 81315.97 KB |        8.54 |
| CsvHelper_ReadTyped_Stream                                | 100000   | 45,436.4 μs | 529.60 μs |   495.38 μs | 45,340.2 μs |  1.07 |    0.02 | 4545.4545 | 181.8182 |        - | 37602.79 KB |        3.95 |
| CsvHelper_ReadTyped_SemicolonHighQuote                    | 100000   | 50,806.8 μs | 998.15 μs |   980.32 μs | 50,945.2 μs |  1.20 |    0.03 | 4600.0000 | 200.0000 |        - | 37603.06 KB |        3.95 |
