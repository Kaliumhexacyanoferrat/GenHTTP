using System.Buffers;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

// todo: add reading support via PipeReader

public sealed class ContextStream(IBufferWriter<byte> writer) : Stream
{

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() { }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var dest = writer.GetSpan(buffer.Length);
        buffer[..Math.Min(buffer.Length, dest.Length)].CopyTo(dest);
        writer.Advance(Math.Min(buffer.Length, dest.Length));
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

}
