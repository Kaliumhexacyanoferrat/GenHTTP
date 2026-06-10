using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Shared.Types.Body;

public class LengthLimitedStream : Stream, IDrainableStream
{
    private readonly PipeReader _reader;
    
    private readonly long _contentLength;
    private long _bytesRemaining;

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => _contentLength;

    public override long Position
    {
        get => _contentLength - _bytesRemaining;
        set => throw new NotSupportedException("Seeking the body stream is not supported");
    }
    
    public LengthLimitedStream(PipeReader reader, long contentLength)
    {
        _reader = reader;
        _contentLength = contentLength;
        _bytesRemaining = contentLength;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_bytesRemaining <= 0)
        {
            return 0;
        }

        var result = await _reader.ReadAsync(cancellationToken);
        var readBuffer = result.Buffer;

        var toRead = (int)Math.Min(Math.Min(buffer.Length, _bytesRemaining), readBuffer.Length);

        if (toRead == 0)
        {
            _reader.AdvanceTo(readBuffer.Start, readBuffer.End);
            return 0;
        }

        readBuffer.Slice(0, toRead).CopyTo(buffer.Span);

        _reader.AdvanceTo(readBuffer.GetPosition(toRead));

        _bytesRemaining -= toRead;
        return toRead;
    }

    public async ValueTask DrainAsync(CancellationToken cancellationToken = default)
    {
        while (_bytesRemaining > 0)
        {
            var result = await _reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            var toSkip = Math.Min(_bytesRemaining, buffer.Length);
            _reader.AdvanceTo(buffer.GetPosition(toSkip), buffer.End);
            _bytesRemaining -= toSkip;

            if (result.IsCompleted) break;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
        => ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

    public override void Flush()
        => throw new NotSupportedException("Flushing the body stream is not supported");

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("Seeking the body stream is not supported");

    public override void SetLength(long value)
        => throw new NotSupportedException("Length of the body stream cannot be written to");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("Body stream cannot be written to");

}
