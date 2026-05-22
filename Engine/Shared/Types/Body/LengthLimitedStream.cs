namespace GenHTTP.Engine.Shared.Types.Body;

public class LengthLimitedStream(Stream source, long length) : Stream
{
    private long _position = 0;

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => (long)length;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException("Seeking the body stream is not supported");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var maxAvailable = (length - _position);

        var toRead = (count > maxAvailable) ? count : (int)maxAvailable;

        var actuallyRead = source.Read(buffer, offset, toRead);

        _position += actuallyRead;

        return actuallyRead;
    }

    public override void Flush()
        => throw new NotSupportedException("Flushing the body stream is not supported");

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("Seeking the body stream is not supported");

    public override void SetLength(long value)
        => throw new NotSupportedException("Length of the body stream cannot be written to");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("Body stream cannot be written to");

}
