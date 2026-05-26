using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressedResponseContent : IResponseContent, IDisposable
{

    #region Initialization

    public CompressedResponseContent(IResponseContent originalContent, Func<Stream, Stream> generator, AlgorithmName algorithmName)
    {
        OriginalContent = originalContent;
        Generator = generator;
        Encoding = algorithmName.Value;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => OriginalContent.Type;

    public ReadOnlyMemory<byte>? Encoding { get; }

    private IResponseContent OriginalContent { get; }

    private Func<Stream, Stream> Generator { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => OriginalContent.CalculateChecksumAsync();

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        await using var compressingSink = new CompressingSink(sink, Generator);

        await OriginalContent.WriteAsync(compressingSink);
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
