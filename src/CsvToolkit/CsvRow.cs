using CsvToolkit.Internal;

namespace CsvToolkit;

public readonly struct CsvRow
{
    private readonly ReadOnlyMemory<char> _buffer;
    private readonly CsvFieldToken[]? _fields;
    private readonly int _fieldCount;

    internal CsvRow(ReadOnlyMemory<char> buffer, CsvFieldToken[] fields, int fieldCount, long rowIndex, long lineNumber)
    {
        _buffer = buffer;
        _fields = fields;
        _fieldCount = fieldCount;
        RowIndex = rowIndex;
        LineNumber = lineNumber;
    }

    public long RowIndex { get; }

    public long LineNumber { get; }

    public int FieldCount => _fieldCount;

    public ReadOnlySpan<char> this[int index] => GetFieldSpan(index);

    public ReadOnlySpan<char> GetFieldSpan(int index)
    {
        var memory = GetFieldMemory(index);
        return memory.Span;
    }

    public ReadOnlyMemory<char> GetFieldMemory(int index)
    {
        EnsureIndex(index);
        var token = _fields![index];
        return _buffer.Slice(token.Start, token.Length);
    }

    public string GetFieldString(int index)
    {
        return GetFieldMemory(index).ToString();
    }

    private void EnsureIndex(int index)
    {
        if (_fields is null || index < 0 || index >= _fieldCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
