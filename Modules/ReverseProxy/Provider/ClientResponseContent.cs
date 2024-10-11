using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.ReverseProxy.Provider;

internal sealed class ClientResponseContent : IResponseContent, IDisposable
{
    private bool _Disposed;

    #region Get/Setters

    private HttpResponseMessage Message { get; }

    public ulong? Length
    {
        get
        {
                var length = Message.Content.Headers.ContentLength;

                if (length != null)
                {
                    return (ulong)length;
                }

                return null;
            }
    }

    #endregion

    #region Initialization

    public ClientResponseContent(HttpResponseMessage message)
    {
            Message = message;
        }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new();

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
            using var source = await Message.Content.ReadAsStreamAsync();

            await source.CopyPooledAsync(target, bufferSize);
        }

    #endregion

    #region Disposal

    private void Dispose(bool disposing)
    {
            if (!_Disposed)
            {
                if (disposing)
                {
                    Message.Dispose();
                }

                _Disposed = true;
            }
        }

    public void Dispose()
    {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    #endregion

}
