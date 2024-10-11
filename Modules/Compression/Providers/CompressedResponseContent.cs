using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressedResponseContent : IResponseContent, IDisposable
{

    #region Initialization

    public CompressedResponseContent(IResponseContent originalContent, Func<Stream, Stream> generator)
    {
        OriginalContent = originalContent;
        Generator = generator;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    private IResponseContent OriginalContent { get; }

    private Func<Stream, Stream> Generator { get; }

    #endregion

    #region Functionality

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await using var compressed = Generator(target);

        await OriginalContent.WriteAsync(compressed, bufferSize);
    }

    public ValueTask<ulong?> CalculateChecksumAsync() => OriginalContent.CalculateChecksumAsync();

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                if (OriginalContent is IDisposable disposableContent)
                {
                    disposableContent.Dispose();
                }
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
