namespace GenHTTP.Modules.IO.Streaming;

using System.Buffers;
using System.IO;

public sealed class StreamBufferWriter : IBufferWriter<byte>
{
    private readonly Stream _stream;
    private byte[] _buffer;

    public StreamBufferWriter(Stream stream, int bufferSize = 8192)
    {
        _stream = stream;
        _buffer = new byte[bufferSize];
    }

    public void Advance(int count)
    {
        _stream.Write(_buffer, 0, count);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer;
    }

    private void Ensure(int sizeHint)
    {
        if (sizeHint > _buffer.Length)
        {
            _buffer = new byte[Math.Max(sizeHint, _buffer.Length * 2)];
        }
    }

}
