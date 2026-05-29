using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressedResponseContent : IResponseContent, IDisposable
{

    #region Initialization

    public CompressedResponseContent(IResponseContent originalContent, Func<IResponseSink, IResponseSink> sinkFactory, AlgorithmName algorithmName)
    {
        OriginalContent = originalContent;
        SinkFactory = sinkFactory;
        Encoding = algorithmName.Value;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => OriginalContent.Type;

    public ReadOnlyMemory<byte>? Encoding { get; }

    private IResponseContent OriginalContent { get; }

    private Func<IResponseSink, IResponseSink> SinkFactory { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => OriginalContent.CalculateChecksumAsync();

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        var compressingSink = SinkFactory(sink);

        try
        {
            await OriginalContent.WriteAsync(compressingSink);
        }
        finally
        {
            if (compressingSink is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (compressingSink is IDisposable disposable)
                disposable.Dispose();
        }
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (OriginalContent is IDisposable disposableContent)
                {
                    disposableContent.Dispose();
                }
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
