using System.IO.Pipelines;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class WritingStream(PipeWriter writer, Stream readingStream) : Stream
{

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
        => FlushAsync().GetAwaiter().GetResult();

    public override async Task FlushAsync(CancellationToken cancellationToken)
        => await writer.FlushAsync(cancellationToken);

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var dest = writer.GetSpan(buffer.Length);
        buffer[..Math.Min(buffer.Length, dest.Length)].CopyTo(dest);
        writer.Advance(Math.Min(buffer.Length, dest.Length));
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override int Read(byte[] buffer, int offset, int count)
        => readingStream.Read(buffer, offset, count);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        => readingStream.ReadAsync(buffer, cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => readingStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

}
