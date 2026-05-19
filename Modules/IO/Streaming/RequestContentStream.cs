using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.IO.Streaming;

// todo: eliminate this class?

public class RequestContentStream(IRawRequestBody body) : Stream
{
    private ReadOnlyMemory<byte> _remaining = ReadOnlyMemory<byte>.Empty;

    private bool _completed;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException("Unable to determine the body length");

    public override long Position
    {
        get => throw new NotSupportedException("Position is not supported by this stream");
        set => throw new NotSupportedException("Position is not supported by this stream");
    }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("Seeking is not supported by this stream");

    public override void SetLength(long value)
        => throw new NotSupportedException("Setting the stream length is not supported by this stream");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("Request body is read-only");

    public override int Read(byte[] buffer, int offset, int count) // todo ...
        => ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None)
            .AsTask()
            .GetAwaiter()
            .GetResult();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        if (!_remaining.IsEmpty)
        {
            return Drain(buffer);
        }

        if (_completed)
        {
            return 0;
        }

        var chunk = await body.TryReadAsync().ConfigureAwait(false);

        if (chunk is null)
        {
            _completed = true;
            return 0;
        }

        _remaining = chunk.Value;
        return Drain(buffer);
    }

    private int Drain(Memory<byte> buffer)
    {
        var toCopy = Math.Min(_remaining.Length, buffer.Length);
        _remaining[..toCopy].CopyTo(buffer);
        _remaining = _remaining[toCopy..];
        return toCopy;
    }

}
