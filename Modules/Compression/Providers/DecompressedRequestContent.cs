using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Wraps an incoming request stream to transparently decompress the content.
/// </summary>
public sealed class DecompressedRequestContent : Stream
{
    private readonly Stream _decompressedStream;

    #region Initialization

    public DecompressedRequestContent(Stream requestContent, Func<Stream, Stream> decompressor)
    {
        _decompressedStream = decompressor(requestContent);
    }

    #endregion

    #region Stream Implementation

    public override bool CanRead => _decompressedStream.CanRead;

    public override bool CanSeek => _decompressedStream.CanSeek;

    public override bool CanWrite => false;

    public override long Length => _decompressedStream.Length;

    public override long Position
    {
        get => _decompressedStream.Position;
        set => _decompressedStream.Position = value;
    }

    public override void Flush() => _decompressedStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
        => _decompressedStream.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer)
        => _decompressedStream.Read(buffer);

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await _decompressedStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => await _decompressedStream.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin)
        => _decompressedStream.Seek(offset, origin);

    public override void SetLength(long value)
        => _decompressedStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _decompressedStream.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _decompressedStream.DisposeAsync();
        await base.DisposeAsync();
    }

    #endregion
}
