using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class WritingStream : Stream
{

    private readonly IBufferWriter<byte> _writer;

    private readonly PipeWriter _flusher;

    private readonly Stream _readingStream;

    public WritingStream(PipeWriter writer, Stream readingStream)
        : this(writer, writer, readingStream) { }

    public WritingStream(IBufferWriter<byte> writer, PipeWriter flusher, Stream readingStream)
    {
        _writer = writer;
        _flusher = flusher;
        _readingStream = readingStream;
    }

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
        => await _flusher.FlushAsync(cancellationToken);

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var dest = _writer.GetSpan(buffer.Length);
        buffer[..Math.Min(buffer.Length, dest.Length)].CopyTo(dest);
        _writer.Advance(Math.Min(buffer.Length, dest.Length));
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override int Read(byte[] buffer, int offset, int count)
        => _readingStream.Read(buffer, offset, count);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        => _readingStream.ReadAsync(buffer, cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _readingStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

}
