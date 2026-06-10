using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.ReverseProxy.Http;

internal sealed class ClientResponseContent(HttpResponseMessage message) : IResponseContent, IDisposable
{
    private bool _disposed;

    #region Get/Setters

    private HttpResponseMessage Message { get; } = message;

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

    public ContentType? Type
    {
        get
        {
            var type = Message.Content.Headers.ContentType;

            if (type != null)
            {
                return new(type.ToString());
            }

            return ContentType.ApplicationOctetStream;
        }
    }

    public ReadOnlyMemory<byte>? Encoding
    {
        get
        {
            var encoding = Message.Content.Headers.ContentEncoding.FirstOrDefault();

            if (encoding != null)
            {
                return System.Text.Encoding.ASCII.GetBytes(encoding);
            }

            return null;
        }
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new();

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await using var source = await Message.Content.ReadAsStreamAsync();

        await source.CopyPooledAsync(target, bufferSize);
    }

    public ValueTask WriteAsync(IResponseSink sink) => WriteAsync(sink.Stream, 8192);

    #endregion

    #region Disposal

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Message.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }

    #endregion

}
