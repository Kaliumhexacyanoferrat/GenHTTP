using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class StreamContent : IResponseContent, IDisposable
{
    private readonly Func<ValueTask<ulong?>> _checksumProvider;

    private readonly ulong? _knownLengh;

    #region Initialization

    public StreamContent(Stream content, ulong? knownLength, Func<ValueTask<ulong?>> checksumProvider)
    {
        Content = content;

        _knownLengh = knownLength;
        _checksumProvider = checksumProvider;
    }

    #endregion

    #region Get-/Setters

    private Stream Content { get; }

    public ulong? Length
    {
        get
        {
            if (_knownLengh != null)
            {
                return _knownLengh;
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

    public ValueTask<ulong?> CalculateChecksumAsync() => _checksumProvider();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Content.CopyPooledAsync(target, bufferSize);

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
