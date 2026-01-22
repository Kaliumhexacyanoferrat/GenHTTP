namespace GenHTTP.Modules.Archives.Tree;

public class ArchiveEntryStream : Stream
{
    private readonly ArchiveHandle _handle;

    public ArchiveEntryStream(ArchiveHandle handle)
    {
        _handle = handle;
    }

    public override bool CanRead => _handle.Content.CanRead;

    public override bool CanSeek => _handle.Content.CanSeek;

    public override bool CanWrite => _handle.Content.CanWrite;

    public override long Length => _handle.Content.Length;

    public override long Position
    {
        get => _handle.Content.Position;
        set => _handle.Content.Position = value;
    }

    public override void Flush() => _handle.Content.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _handle.Content.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _handle.Content.Seek(offset, origin);

    public override void SetLength(long value) => _handle.Content.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) =>  _handle.Content.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _handle.Content.Dispose();
            _handle.Handle.Dispose();
        }

        base.Dispose(disposing);
    }

}
