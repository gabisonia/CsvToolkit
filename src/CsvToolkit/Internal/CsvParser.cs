using System.Buffers;

namespace CsvToolkit.Internal;

internal sealed class CsvParser(ICsvCharInput input, CsvOptions options) : IDisposable, IAsyncDisposable
{
    private readonly CsvRowBuffer _rowBuffer = new(options.CharBufferSize);
    private readonly char[] _readBuffer = ArrayPool<char>.Shared.Rent(options.CharBufferSize);
    private int _readPosition;
    private int _readLength;
    private int _pushback = -1;

    public CsvRow CurrentRow { get; private set; }

    private long RowIndex { get; set; }

    private long LineNumber { get; set; } = 1;

    public string? DetectedNewLine { get; private set; }

    public bool TryReadRow(out CsvRow row)
    {
        var read = TryReadRowCoreAsync(useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        row = CurrentRow;
        return read;
    }

    public ValueTask<bool> TryReadRowAsync(CancellationToken cancellationToken)
    {
        return TryReadRowCoreAsync(useAsync: true, cancellationToken);
    }

    public void Dispose()
    {
        _rowBuffer.Dispose();
        ArrayPool<char>.Shared.Return(_readBuffer);
        input.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _rowBuffer.Dispose();
        ArrayPool<char>.Shared.Return(_readBuffer);
        await input.DisposeAsync().ConfigureAwait(false);
    }

    private async ValueTask<bool> TryReadRowCoreAsync(bool useAsync, CancellationToken cancellationToken)
    {
        _rowBuffer.Reset();

        var inQuotes = false;
        var afterClosingQuote = false;
        var fieldWasQuoted = false;
        var consumedAnything = false;

        while (true)
        {
            var code = useAsync
                ? await ReadCharAsync(cancellationToken).ConfigureAwait(false)
                : ReadChar();

            if (code < 0)
            {
                if (!consumedAnything && _rowBuffer.FieldCount == 0 && _rowBuffer.CurrentFieldLength == 0)
                {
                    return false;
                }

                if (inQuotes)
                {
                    HandleBadData(
                        _rowBuffer.FieldCount,
                        "Unexpected end of file while inside a quoted field.",
                        _rowBuffer.CurrentFieldMemory);
                }

                _rowBuffer.CompleteField(fieldWasQuoted, options.TrimOptions);
                if (options.IgnoreBlankLines && _rowBuffer.IsBlankLine())
                {
                    return false;
                }

                CurrentRow = _rowBuffer.ToRow(RowIndex, LineNumber);
                RowIndex++;
                return true;
            }

            consumedAnything = true;
            var ch = (char)code;

            if (inQuotes)
            {
                if (options.Escape != options.Quote && ch == options.Escape)
                {
                    var escaped = useAsync
                        ? await ReadCharAsync(cancellationToken).ConfigureAwait(false)
                        : ReadChar();

                    if (escaped == options.Quote)
                    {
                        _rowBuffer.Append(options.Quote);
                        continue;
                    }

                    if (escaped >= 0)
                    {
                        PushBack(escaped);
                    }

                    _rowBuffer.Append(ch);
                    continue;
                }

                if (ch == options.Quote)
                {
                    var next = useAsync
                        ? await ReadCharAsync(cancellationToken).ConfigureAwait(false)
                        : ReadChar();

                    if (next == options.Quote)
                    {
                        _rowBuffer.Append(options.Quote);
                        continue;
                    }

                    inQuotes = false;
                    afterClosingQuote = true;

                    if (next >= 0)
                    {
                        PushBack(next);
                    }

                    continue;
                }

                _rowBuffer.Append(ch);
                continue;
            }

            if (afterClosingQuote)
            {
                if (ch == options.Delimiter)
                {
                    _rowBuffer.CompleteField(true, options.TrimOptions);
                    fieldWasQuoted = false;
                    afterClosingQuote = false;
                    continue;
                }

                if (ch == '\r' || ch == '\n')
                {
                    await ConsumeNewLineSuffixAsync(ch, useAsync, cancellationToken).ConfigureAwait(false);
                    _rowBuffer.CompleteField(true, options.TrimOptions);
                    if (options.IgnoreBlankLines && _rowBuffer.IsBlankLine())
                    {
                        ResetRowState(ref consumedAnything, ref inQuotes, ref afterClosingQuote, ref fieldWasQuoted);
                        continue;
                    }

                    CurrentRow = _rowBuffer.ToRow(RowIndex, LineNumber);
                    RowIndex++;
                    return true;
                }

                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                HandleBadData(
                    _rowBuffer.FieldCount,
                    "Unexpected character after closing quote.",
                    _rowBuffer.CurrentFieldMemory);

                afterClosingQuote = false;
                _rowBuffer.Append(ch);
                continue;
            }

            if (ch == options.Delimiter)
            {
                _rowBuffer.CompleteField(fieldWasQuoted, options.TrimOptions);
                fieldWasQuoted = false;
                continue;
            }

            if (ch == options.Quote)
            {
                if (_rowBuffer.CurrentFieldLength == 0)
                {
                    inQuotes = true;
                    fieldWasQuoted = true;
                    continue;
                }

                HandleBadData(
                    _rowBuffer.FieldCount,
                    "Unexpected quote in unquoted field.",
                    _rowBuffer.CurrentFieldMemory);

                _rowBuffer.Append(ch);
                continue;
            }

            if (ch == '\r' || ch == '\n')
            {
                await ConsumeNewLineSuffixAsync(ch, useAsync, cancellationToken).ConfigureAwait(false);
                _rowBuffer.CompleteField(fieldWasQuoted, options.TrimOptions);
                if (options.IgnoreBlankLines && _rowBuffer.IsBlankLine())
                {
                    ResetRowState(ref consumedAnything, ref inQuotes, ref afterClosingQuote, ref fieldWasQuoted);
                    continue;
                }

                CurrentRow = _rowBuffer.ToRow(RowIndex, LineNumber);
                RowIndex++;
                return true;
            }

            if (_rowBuffer.CurrentFieldLength == 0 &&
                (options.TrimOptions & CsvTrimOptions.TrimStart) != 0 &&
                char.IsWhiteSpace(ch))
            {
                continue;
            }

            _rowBuffer.Append(ch);
        }
    }

    private void ResetRowState(
        ref bool consumedAnything,
        ref bool inQuotes,
        ref bool afterClosingQuote,
        ref bool fieldWasQuoted)
    {
        _rowBuffer.Reset();
        consumedAnything = false;
        inQuotes = false;
        afterClosingQuote = false;
        fieldWasQuoted = false;
    }

    private async ValueTask ConsumeNewLineSuffixAsync(char ch, bool useAsync, CancellationToken cancellationToken)
    {
        if (ch == '\r')
        {
            var next = useAsync
                ? await ReadCharAsync(cancellationToken).ConfigureAwait(false)
                : ReadChar();

            if (next != '\n' && next >= 0)
            {
                PushBack(next);
            }

            if (DetectedNewLine is null)
            {
                DetectedNewLine = next == '\n' ? "\r\n" : "\r";
            }
        }
        else if (DetectedNewLine is null)
        {
            DetectedNewLine = "\n";
        }

        LineNumber++;
    }

    private void HandleBadData(int fieldIndex, string message, ReadOnlyMemory<char> rawField)
    {
        if (options.ReadMode == CsvReadMode.Strict)
        {
            throw new CsvException(message, RowIndex, LineNumber, fieldIndex);
        }

        options.BadDataFound?.Invoke(new CsvBadDataContext(RowIndex, LineNumber, fieldIndex, message, rawField));
    }

    private void PushBack(int value)
    {
        _pushback = value;
    }

    private int ReadChar()
    {
        if (_pushback >= 0)
        {
            var pushed = _pushback;
            _pushback = -1;
            return pushed;
        }

        if (_readPosition >= _readLength)
        {
            _readLength = input.Read(_readBuffer.AsSpan());
            _readPosition = 0;
            if (_readLength == 0)
            {
                return -1;
            }
        }

        return _readBuffer[_readPosition++];
    }

    private async ValueTask<int> ReadCharAsync(CancellationToken cancellationToken)
    {
        if (_pushback >= 0)
        {
            var pushed = _pushback;
            _pushback = -1;
            return pushed;
        }

        if (_readPosition >= _readLength)
        {
            _readLength = await input.ReadAsync(_readBuffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            _readPosition = 0;
            if (_readLength == 0)
            {
                return -1;
            }
        }

        return _readBuffer[_readPosition++];
    }
}