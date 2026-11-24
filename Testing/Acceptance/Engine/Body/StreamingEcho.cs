using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

/// <summary>
/// Streams the request body straight back as the response, as an echo service does: the response
/// is written while the body is still being read, and the length of neither is known up front.
/// </summary>
internal sealed class StreamingEchoHandler : IHandler
{

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var body = request.GetBody();

        if (body == null)
        {
            return new(request.Respond().Status(ResponseStatus.BadRequest).Build());
        }

        return new(request.Respond()
                          .Content(body.AsStream(), ContentType.ApplicationOctetStream)
                          .Build());
    }

}

/// <summary>
/// Sends the payload in writes of the given sizes, each flushed on its own, so that every write
/// travels as a chunk (or data frame) of its own. Declares its length only if asked to.
/// </summary>
internal sealed class UnevenContent(byte[] payload, int[] sizes, bool declareLength = false) : HttpContent
{

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var offset = 0;
        var i = 0;

        while (offset < payload.Length)
        {
            var size = Math.Min(sizes[i++ % sizes.Length], payload.Length - offset);

            await stream.WriteAsync(payload.AsMemory(offset, size));
            await stream.FlushAsync();

            offset += size;
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = payload.Length;
        return declareLength;
    }

}
