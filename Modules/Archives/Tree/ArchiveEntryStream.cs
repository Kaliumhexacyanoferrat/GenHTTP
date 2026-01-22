namespace GenHTTP.Modules.Archives.Tree;

internal sealed class ArchiveEntryStream(ArchiveHandle handle) : Stream
{

    #region Get-/Setters

    public override bool CanRead => handle.Content.CanRead;

    public override bool CanSeek => handle.Content.CanSeek;

    public override bool CanWrite => handle.Content.CanWrite;

    public override long Length => handle.Content.Length;

    public override long Position
    {
        get => handle.Content.Position;
        set => handle.Content.Position = value;
    }

    #endregion

    #region Functionality

    public override void Flush() => handle.Content.Flush();

    public override int Read(byte[] buffer, int offset, int count) => handle.Content.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => handle.Content.Seek(offset, origin);

    public override void SetLength(long value) => handle.Content.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => handle.Content.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            handle.Content.Dispose();
            handle.Handle.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

}
