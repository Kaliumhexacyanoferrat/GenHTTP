namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Provides the body of a request as a read-only stream.
/// </summary>
internal sealed class BodyStream(IBodyFraming framing) : Stream
{

    #region Get-/Setters

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => framing.Length ?? throw new NotSupportedException("Length is not known for chunked streams");

    public override long Position
    {
        get => framing.Position;
        set => throw new NotSupportedException("Seeking the body stream is not supported");
    }

    #endregion

    #region Functionality

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => framing.ReadAsync(buffer, cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => framing.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count)
        => framing.ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

    public override void Flush()
        => throw new NotSupportedException("Flushing the body stream is not supported");

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("Seeking the body stream is not supported");

    public override void SetLength(long value)
        => throw new NotSupportedException("Length of the body stream cannot be written to");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("Body stream cannot be written to");

    #endregion

}
