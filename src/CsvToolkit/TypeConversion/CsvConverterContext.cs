using System.Globalization;

namespace CsvToolkit.TypeConversion;

public readonly record struct CsvConverterContext(
    CultureInfo CultureInfo,
    long RowIndex,
    int FieldIndex,
    string? ColumnName);
