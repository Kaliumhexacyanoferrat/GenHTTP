using System.Buffers;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class WriterStreamAdapter : Stream
{
    private readonly IBufferWriter<byte> _writer;

    #region Get-/Setters
    
    public override bool CanRead => false;
    
    public override bool CanSeek => false;
    
    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();
    
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    
    #endregion
    
    #region Initialization
    
    public WriterStreamAdapter(IBufferWriter<byte> writer) => _writer = writer;

    #endregion
    
    #region Functionality

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (buffer.Length > 0)
        {
            var destination = _writer.GetSpan();
            var copyLen = Math.Min(buffer.Length, destination.Length);
            buffer.Slice(0, copyLen).CopyTo(destination);
            _writer.Advance(copyLen);
            buffer = buffer.Slice(copyLen);
        }
    }

    public override void Write(byte[] buffer, int offset, int count) 
        => Write(buffer.AsSpan(offset, count));

    public override void Flush() { }
    
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    
    public override void SetLength(long value) => throw new NotSupportedException();
    
    #endregion
    
}
