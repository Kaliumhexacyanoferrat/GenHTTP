using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class StreamContent : IResponseContent, IDisposable
{
    private readonly Func<ValueTask<ulong?>> _ChecksumProvider;

    private readonly ulong? _KnownLengh;

    #region Initialization

    public StreamContent(Stream content, ulong? knownLength, Func<ValueTask<ulong?>> checksumProvider)
    {
        Content = content;

        _KnownLengh = knownLength;
        _ChecksumProvider = checksumProvider;
    }

    #endregion

    #region Get-/Setters

    private Stream Content { get; }

    public ulong? Length
    {
        get
        {
            if (_KnownLengh != null)
            {
                return _KnownLengh;
            }

            if (Content.CanSeek)
            {
                return (ulong)Content.Length;
            }

            return null;
        }
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => _ChecksumProvider();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Content.CopyPooledAsync(target, bufferSize);

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                Content.Dispose();
            }

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion

}
