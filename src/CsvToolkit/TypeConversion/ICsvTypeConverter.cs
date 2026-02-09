namespace CsvToolkit.TypeConversion;

public interface ICsvTypeConverter<T>
{
    bool TryParse(ReadOnlySpan<char> source, in CsvConverterContext context, out T value);

    string Format(T value, in CsvConverterContext context);
}
