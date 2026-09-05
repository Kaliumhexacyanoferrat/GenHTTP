using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>Frames writes as chunked transfer encoding in place, with no staging buffer.</summary>
// Same arithmetic as Shared's Http1ChunkedWriter, which takes an IClientContext whose Stream leads
// back to WritingStream. Kept here rather than reshaping a type two other engines depend on.
internal sealed class Http1ChunkedWriter(PipeWriter writer) : IBufferWriter<byte>
{
    private const int MaxHeaderSize = 10; // 8 hex digits + CRLF
    private const int TrailerSize = 2;    // CRLF

    private Memory<byte> _activeMemory;

    // Hands out the payload window, leaving room for the chunk header and its trailing CRLF.
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        _activeMemory = writer.GetMemory(Math.Max(sizeHint, 1) + MaxHeaderSize + TrailerSize);

        return _activeMemory.Slice(MaxHeaderSize, _activeMemory.Length - MaxHeaderSize - TrailerSize);
    }

    // The same window as a span.
    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

    // Frames what was written: size in front, CRLF behind, then commits the whole chunk.
    public void Advance(int count)
    {
        if (count == 0)
        {
            return;
        }

        if (_activeMemory.IsEmpty)
        {
            throw new InvalidOperationException("GetMemory() or GetSpan() must be called before Advance().");
        }

        var span = _activeMemory.Span;

        WriteHex((uint)count, span);

        var trailerOffset = MaxHeaderSize + count;

        span[trailerOffset] = (byte)'\r';
        span[trailerOffset + 1] = (byte)'\n';

        writer.Advance(MaxHeaderSize + count + TrailerSize);

        _activeMemory = default;
    }

    // Writes the zero-length chunk that ends the body.
    public void Finish()
    {
        var span = writer.GetSpan(5);

        "0\r\n\r\n"u8.CopyTo(span);

        writer.Advance(5);
    }

    // The chunk size as eight fixed hex digits, so the header is always the same width.
    private static void WriteHex(uint value, Span<byte> dest)
    {
        for (var pos = 7; pos >= 0; pos--)
        {
            var digit = value & 0xF;

            dest[pos] = digit < 10 ? (byte)('0' + digit) : (byte)('A' + digit - 10);

            value >>= 4;
        }

        dest[8] = (byte)'\r';
        dest[9] = (byte)'\n';
    }
}
