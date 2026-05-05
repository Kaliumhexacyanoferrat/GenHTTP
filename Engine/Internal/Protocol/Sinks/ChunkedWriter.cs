using System.Buffers;

using GenHTTP.Engine.Internal.Context;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

internal sealed class ChunkedWriter(ClientContext context) : IBufferWriter<byte>
{
    private const int HeaderSize = 10;
    private const int TrailerSize = 2;
    private const int Overhead = HeaderSize + TrailerSize;

    private Memory<byte> _activeMemory;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        _activeMemory = context.Writer.GetMemory(Math.Max(sizeHint, 1) + Overhead);

        return _activeMemory.Slice(HeaderSize, _activeMemory.Length - Overhead);
    }

    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

    public void Advance(int count)
    {
        if (count == 0) return;

        var span = _activeMemory.Span;

        WriteHex8((uint)count, span);

        span[HeaderSize + count] = (byte)'\r';
        span[HeaderSize + count + 1] = (byte)'\n';

        context.Writer.Advance(HeaderSize + count + TrailerSize);
        
        _activeMemory = default;
    }

    public void Finish()
    {
        var writer = context.Writer;
        
        var span = writer.GetSpan(5);
        
        span[0] = (byte)'0';
        span[1] = (byte)'\r';
        span[2] = (byte)'\n';
        span[3] = (byte)'\r';
        span[4] = (byte)'\n';
        
        writer.Advance(5);
    }

    private static void WriteHex8(uint value, Span<byte> dest)
    {
        const string hex = "0123456789ABCDEF";

        for (var i = 7; i >= 0; i--)
        {
            dest[i] = (byte)hex[(int)(value & 0xF)];
            value >>= 4;
        }

        dest[8] = (byte)'\r';
        dest[9] = (byte)'\n';
    }

}
