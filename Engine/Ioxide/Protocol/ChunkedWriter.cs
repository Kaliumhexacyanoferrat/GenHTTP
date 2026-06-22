using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Wraps a <see cref="PipeWriter"/> and frames every <see cref="Advance"/> as an HTTP/1.1
/// transfer-encoding chunk (size in hex + CRLF + data + CRLF). Ported from GenHTTP's Internal
/// engine (ChunkedWriter): a fixed 8-hex-digit size header is reserved ahead of the payload so
/// the chunk can be framed in place without a second copy.
/// </summary>
internal sealed class ChunkedWriter(PipeWriter writer) : IBufferWriter<byte>
{
    private const int MaxHeaderSize = 10; // 8 hex digits + CRLF
    private const int TrailerSize = 2;    // CRLF

    private Memory<byte> _activeMemory;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        _activeMemory = writer.GetMemory(Math.Max(sizeHint, 1) + MaxHeaderSize + TrailerSize);

        return _activeMemory.Slice(MaxHeaderSize, _activeMemory.Length - MaxHeaderSize - TrailerSize);
    }

    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

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

    /// <summary>Writes the terminating zero-length chunk.</summary>
    public void Finish()
    {
        var span = writer.GetSpan(5);

        "0\r\n\r\n"u8.CopyTo(span);

        writer.Advance(5);
    }

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
