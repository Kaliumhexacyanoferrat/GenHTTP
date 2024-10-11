using System.Buffers;
using GenHTTP.Api.Protocol;
using PooledAwait;

namespace GenHTTP.Engine.Protocol.Parser;

internal sealed class RequestScanner
{

    internal RequestScanner()
    {
        Current = RequestToken.None;
        Mode = ScannerMode.Words;
    }

    internal RequestToken Current { get; private set; }

    internal ReadOnlySequence<byte> Value { get; private set; }

    internal ScannerMode Mode { get; set; }

    internal async PooledValueTask<bool> Next(RequestBuffer buffer, RequestToken expectedToken, bool allowNone = false, bool includeWhitespace = false)
    {
        var read = await Next(buffer, false, includeWhitespace);

        if (allowNone && read == RequestToken.None)
        {
            return false;
        }

        if (read != expectedToken)
        {
            throw new ProtocolException($"Unexpected token '{read}' (expected '{expectedToken}')");
        }

        return true;
    }

    internal async PooledValueTask<RequestToken> Next(RequestBuffer buffer, bool forceRead = false, bool includeWhitespace = false)
    {
        // ensure we have data to be scanned
        if (await Fill(buffer, forceRead))
        {
            var found = ScanData(buffer, includeWhitespace);

            if (found != null)
            {
                return Current = found.Value;
            }
        }

        // did not recognize any tokens, probably due to missing input data
        if (!forceRead)
        {
            return await Next(buffer, true, includeWhitespace);
        }

        return Current = RequestToken.None;
    }

    private RequestToken? ScanData(RequestBuffer buffer, bool includeWhitespace = false)
    {
        if (SkipWhitespace(buffer) && includeWhitespace)
        {
            Value = new ReadOnlySequence<byte>();
            return RequestToken.Word;
        }

        if (Mode == ScannerMode.Words)
        {
            if (ReadTo(buffer, ' ', '\r'))
            {
                return RequestToken.Word;
            }
            if (ReadTo(buffer, '\r', skipAdditionally: 1))
            {
                return RequestToken.Word;
            }
        }
        else if (Mode == ScannerMode.Path)
        {
            if (ReadTo(buffer, '?', '\r'))
            {
                return RequestToken.PathWithQuery;
            }
            if (ReadTo(buffer, ' ', '\r'))
            {
                return RequestToken.Path;
            }
        }
        else if (Mode == ScannerMode.HeaderKey)
        {
            if (IsNewLine(buffer))
            {
                return RequestToken.NewLine;
            }
            if (ReadTo(buffer, ':', '\r', 1))
            {
                return RequestToken.Word;
            }
        }
        else if (Mode == ScannerMode.HeaderValue)
        {
            if (ReadTo(buffer, '\r', skipAdditionally: 1))
            {
                return RequestToken.Word;
            }
        }

        return null;
    }

    private static bool SkipWhitespace(RequestBuffer buffer)
    {
        var count = 0;
        var done = false;

        foreach (var memory in buffer.Data)
        {
            for (var i = 0; i < memory.Length; i++)
            {
                if (memory.Span[i] == (byte)' ')
                {
                    count++;
                }
                else
                {
                    done = true;
                    break;
                }
            }

            if (done)
            {
                break;
            }
        }

        if (count > 0)
        {
            buffer.Advance(count);
        }

        return count > 0;
    }

    private static bool IsNewLine(RequestBuffer buffer)
    {
        if (buffer.Data.FirstSpan[0] == (byte)'\r')
        {
            buffer.Advance(2);
            return true;
        }

        return false;
    }

    private bool ReadTo(RequestBuffer buffer, char delimiter, char? boundary = null, byte skipAdditionally = 0)
    {
        var reader = new SequenceReader<byte>(buffer.Data);

        if (reader.TryReadTo(out ReadOnlySequence<byte> value, (byte)delimiter))
        {
            if (boundary != null)
            {
                var boundaryReader = new SequenceReader<byte>(buffer.Data);

                if (boundaryReader.TryReadTo(out ReadOnlySequence<byte> boundaryData, (byte)boundary))
                {
                    if (boundaryData.Length < value.Length)
                    {
                        return false;
                    }
                }
            }

            Value = value;
            buffer.Advance(value.Length + 1 + skipAdditionally);

            return true;
        }

        return false;
    }

    private static async PooledValueTask<bool> Fill(RequestBuffer buffer, bool force = false)
    {
        if (buffer.ReadRequired || force)
        {
            await buffer.ReadAsync(force);
        }

        return !buffer.Data.IsEmpty;
    }
}
