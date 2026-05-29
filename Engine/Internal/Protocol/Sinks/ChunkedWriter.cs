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

        var headerLength = WriteHex((uint)count, span);

        if (headerLength != MaxHeaderSize)
        {
            span.Slice(MaxHeaderSize, count)
                .CopyTo(span.Slice(headerLength));
        }

        var trailerOffset = headerLength + count;

        span[trailerOffset] = (byte)'\r';
        span[trailerOffset + 1] = (byte)'\n';

        context.Writer.Advance(headerLength + count + TrailerSize);

        _activeMemory = default;
    }

    public void Finish()
    {
        var span = context.Writer.GetSpan(5);

        "0\r\n\r\n"u8.CopyTo(span);

        context.Writer.Advance(5);
    }

    private static int WriteHex(uint value, Span<byte> dest)
    {
        const int end = 8;

        var pos = end;

        do
        {
            var digit = value & 0xF;

            dest[--pos] = digit < 10 ? (byte)('0' + digit) : (byte)('A' + digit - 10);

            value >>= 4;
        }
        while (value != 0);

        var length = end - pos;

        dest.Slice(pos, length).CopyTo(dest);

        dest[length] = (byte)'\r';
        dest[length + 1] = (byte)'\n';

        return length + 2;
    }
    
}