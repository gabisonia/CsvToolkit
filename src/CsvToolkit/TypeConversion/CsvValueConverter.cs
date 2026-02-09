using System.Globalization;

namespace CsvToolkit.TypeConversion;

internal static class CsvValueConverter
{
    public static bool TryConvert(
        ReadOnlySpan<char> source,
        Type targetType,
        CsvOptions options,
        IUntypedCsvTypeConverter? memberConverter,
        in CsvConverterContext context,
        out object? value)
    {
        if (memberConverter is not null)
        {
            return memberConverter.TryParse(source, targetType, context, out value);
        }

        if (options.Converters.TryGet(targetType, out var converter))
        {
            return converter.TryParse(source, targetType, context, out value);
        }

        if (TryConvertBuiltIn(source, targetType, context.CultureInfo, out value))
        {
            return true;
        }

        if (TryConvertFallback(source, targetType, context.CultureInfo, out value))
        {
            return true;
        }

        value = null;
        return false;
    }

    public static string FormatToString(
        object? value,
        Type valueType,
        CsvOptions options,
        IUntypedCsvTypeConverter? memberConverter,
        in CsvConverterContext context)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (memberConverter is not null)
        {
            return memberConverter.Format(value, valueType, context);
        }

        if (options.Converters.TryGet(valueType, out var converter))
        {
            return converter.Format(value, valueType, context);
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, context.CultureInfo) ?? string.Empty;
        }

        return value.ToString() ?? string.Empty;
    }

    private static bool TryConvertBuiltIn(ReadOnlySpan<char> source, Type targetType, CultureInfo culture, out object? value)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        var effectiveType = nullableType ?? targetType;

        if (source.Length == 0)
        {
            if (nullableType is not null || !effectiveType.IsValueType)
            {
                value = null;
                return true;
            }
        }

        if (effectiveType == typeof(string))
        {
            value = source.ToString();
            return true;
        }

        if (effectiveType == typeof(bool))
        {
            if (bool.TryParse(source, out var boolValue))
            {
                value = boolValue;
                return true;
            }

            if (source.SequenceEqual("1"))
            {
                value = true;
                return true;
            }

            if (source.SequenceEqual("0"))
            {
                value = false;
                return true;
            }
        }

        if (effectiveType == typeof(byte) && byte.TryParse(source, NumberStyles.Integer, culture, out var byteValue))
        {
            value = byteValue;
            return true;
        }

        if (effectiveType == typeof(sbyte) && sbyte.TryParse(source, NumberStyles.Integer, culture, out var sbyteValue))
        {
            value = sbyteValue;
            return true;
        }

        if (effectiveType == typeof(short) && short.TryParse(source, NumberStyles.Integer, culture, out var shortValue))
        {
            value = shortValue;
            return true;
        }

        if (effectiveType == typeof(ushort) && ushort.TryParse(source, NumberStyles.Integer, culture, out var ushortValue))
        {
            value = ushortValue;
            return true;
        }

        if (effectiveType == typeof(int) && int.TryParse(source, NumberStyles.Integer, culture, out var intValue))
        {
            value = intValue;
            return true;
        }

        if (effectiveType == typeof(uint) && uint.TryParse(source, NumberStyles.Integer, culture, out var uintValue))
        {
            value = uintValue;
            return true;
        }

        if (effectiveType == typeof(long) && long.TryParse(source, NumberStyles.Integer, culture, out var longValue))
        {
            value = longValue;
            return true;
        }

        if (effectiveType == typeof(ulong) && ulong.TryParse(source, NumberStyles.Integer, culture, out var ulongValue))
        {
            value = ulongValue;
            return true;
        }

        if (effectiveType == typeof(float) && float.TryParse(source, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var floatValue))
        {
            value = floatValue;
            return true;
        }

        if (effectiveType == typeof(double) && double.TryParse(source, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var doubleValue))
        {
            value = doubleValue;
            return true;
        }

        if (effectiveType == typeof(decimal) && decimal.TryParse(source, NumberStyles.Number, culture, out var decimalValue))
        {
            value = decimalValue;
            return true;
        }

        if (effectiveType == typeof(char))
        {
            if (source.Length == 1)
            {
                value = source[0];
                return true;
            }
        }

        if (effectiveType == typeof(DateTime) && DateTime.TryParse(source, culture, DateTimeStyles.None, out var dateTimeValue))
        {
            value = dateTimeValue;
            return true;
        }

        if (effectiveType == typeof(DateOnly) && DateOnly.TryParse(source, culture, DateTimeStyles.None, out var dateOnlyValue))
        {
            value = dateOnlyValue;
            return true;
        }

        if (effectiveType == typeof(TimeOnly) && TimeOnly.TryParse(source, culture, DateTimeStyles.None, out var timeOnlyValue))
        {
            value = timeOnlyValue;
            return true;
        }

        if (effectiveType == typeof(Guid) && Guid.TryParse(source, out var guidValue))
        {
            value = guidValue;
            return true;
        }

        if (effectiveType.IsEnum)
        {
            var enumText = source.ToString();
            if (Enum.TryParse(effectiveType, enumText, ignoreCase: true, out var enumValue))
            {
                value = enumValue;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryConvertFallback(ReadOnlySpan<char> source, Type targetType, CultureInfo culture, out object? value)
    {
        try
        {
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var effectiveType = nullableType ?? targetType;

            if (source.Length == 0 && nullableType is not null)
            {
                value = null;
                return true;
            }

            if (source.Length == 0 && !effectiveType.IsValueType)
            {
                value = null;
                return true;
            }

            var text = source.ToString();
            value = Convert.ChangeType(text, effectiveType, culture);
            return true;
        }
        catch
        {
            value = null;
            return false;
        }
    }
}
