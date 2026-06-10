using System.Buffers;

using GenHTTP.Engine.Internal.Context;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

internal sealed class ChunkedWriter(ClientContext context) : IBufferWriter<byte>
{
    private const int MaxHeaderSize = 10;
    private const int TrailerSize = 2;

    private Memory<byte> _activeMemory;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        _activeMemory = context.Writer.GetMemory(Math.Max(sizeHint, 1) + MaxHeaderSize + TrailerSize);

        return _activeMemory.Slice(MaxHeaderSize, _activeMemory.Length - MaxHeaderSize - TrailerSize);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
        => GetMemory(sizeHint).Span;

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

        context.Writer.Advance(MaxHeaderSize + count + TrailerSize);

        _activeMemory = default;
    }

    public void Finish()
    {
        var span = context.Writer.GetSpan(5);

        "0\r\n\r\n"u8.CopyTo(span);

        context.Writer.Advance(5);
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