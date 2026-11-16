using System.Net;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// A request body sent without a content-length, the way a client streams one it cannot measure up
/// front: over HTTP/2 and HTTP/3, only the end of the stream says where it stops.
/// </summary>
internal sealed class UnknownLengthContent(byte[] payload) : HttpContent
{

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => stream.WriteAsync(payload).AsTask();

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

}
