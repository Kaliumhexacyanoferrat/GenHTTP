namespace GenHTTP.Adapters.AspNetCore.Context;

// Wraps Kestrel's request body stream (which never supports Length, even when Content-Length
// is known) to match the other engines, whose request body streams do report it.
internal sealed class LengthAwareStream(Stream inner, long length) : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => length;

    public override long Position
    {
        get => throw new NotSupportedException("Seeking the body stream is not supported");
        set => throw new NotSupportedException("Seeking the body stream is not supported");
    }

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => inner.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => inner.ReadAsync(buffer, cancellationToken);

    public override void Flush() => throw new NotSupportedException("Flushing the body stream is not supported");

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("Seeking the body stream is not supported");

    public override void SetLength(long value) => throw new NotSupportedException("Length of the body stream cannot be written to");

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Body stream cannot be written to");

}
