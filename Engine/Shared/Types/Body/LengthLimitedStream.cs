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
    
    public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (_bytesRemaining <= 0)
        {
            return 0;
        }

        var result = await _reader.ReadAsync(cancellationToken);
        var buffer = result.Buffer;

        var toRead = (int)Math.Min(Math.Min(destination.Length, _bytesRemaining), buffer.Length);

        if (toRead == 0)
        {
            _reader.AdvanceTo(buffer.Start, buffer.End);
            return 0;
        }

        buffer.Slice(0, toRead).CopyTo(destination.Span);
        
        _reader.AdvanceTo(buffer.GetPosition(toRead), buffer.End);

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
