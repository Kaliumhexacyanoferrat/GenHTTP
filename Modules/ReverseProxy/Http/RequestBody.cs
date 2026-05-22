using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.ReverseProxy.Http;

public sealed class RequestBody(IRequestBody body) : HttpContent
{

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await body.AsStream().CopyPooledAsync(stream, 8192);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

}
