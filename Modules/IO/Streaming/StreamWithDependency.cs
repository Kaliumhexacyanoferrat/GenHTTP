namespace GenHTTP.Modules.IO.Streaming;

public sealed class StreamWithDependency(Stream content, IDisposable dependency) : Stream
{

    #region Get-/Setters

    public override bool CanRead => content.CanRead;

    public override bool CanSeek => content.CanSeek;

    public override bool CanWrite => content.CanWrite;

    public override long Length => content.Length;

    public override long Position
    {
        get => content.Position;
        set => content.Position = value;
    }

    #endregion

    #region Functionality

    public override void Flush() => content.Flush();

    public override int Read(byte[] buffer, int offset, int count) => content.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => content.Seek(offset, origin);

    public override void SetLength(long value) => content.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => content.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            content.Dispose();
            dependency.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

}
