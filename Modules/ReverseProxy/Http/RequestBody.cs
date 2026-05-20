using System.Net;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ReverseProxy.Http;

public sealed class RequestBody(IRequestBody body) : HttpContent
{

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        ReadOnlyMemory<byte>? chunk;

        while ((chunk = await body.TryReadAsync()) is not null)
        {
            await stream.WriteAsync(chunk.Value);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

}
