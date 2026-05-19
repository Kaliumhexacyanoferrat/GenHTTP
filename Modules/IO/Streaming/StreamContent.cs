using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class StreamContent : IResponseContent, IDisposable
{
    private readonly ChecksumProvider _checksumProvider;

    private readonly ulong? _knownLength;

    #region Initialization

    public StreamContent(Stream content, ContentType contentType, ulong? knownLength, ReadOnlyMemory<byte>? encoding, Func<ValueTask<ulong?>>? checksumProvider)
    {
        Content = content;
        Type = contentType;
        Encoding = encoding;

        _knownLength = knownLength;

        _checksumProvider = new ChecksumProvider(checksumProvider ?? content.CalculateChecksumAsync);
    }

    #endregion

    #region Get-/Setters

    private Stream Content { get; }

    public ContentType? Type { get; }

    public ulong? Length
    {
        get
        {
            if (_knownLength != null)
            {
                return _knownLength;
            }

            if (Content.CanSeek)
            {
                return (ulong)Content.Length;
            }

            return null;
        }
    }

    public ReadOnlyMemory<byte>? Encoding { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => _checksumProvider.Compute();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Content.CopyPooledAsync(target, bufferSize);

    public ValueTask WriteAsync(IResponseSink sink) => WriteAsync(sink.Stream, 8192);

    #endregion

    #region IDisposable Support

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Content.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion

}
