using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;
using GenHTTP.Engine.Ioxide.Protocol.Sinks;

namespace GenHTTP.Engine.Ioxide.Protocol.Responses;

/// <summary>Writes an HTTP/1.1 response onto the connection.</summary>
internal static class Http1Responder
{
    private static readonly byte[] ServerHeader = "Server: ioxide-genhttp\r\n"u8.ToArray();

    // Writes the status line and headers, and the body where the request is owed one.
    internal static async ValueTask WriteAsync(PipeWriter writer, IRequest? request, IResponse response, bool keepAlive, bool headRequest)
    {
        writer.Write(StatusLine.Get(response.Status));

        ResponseSerializer.WriteHeader(writer, response, keepAlive, ServerHeader, Http1DateHeader.Get(), isHttp10: false);

        writer.Write("\r\n"u8);

        if (response.Content is not { } content)
        {
            return;
        }

        try
        {
            // A HEAD, or a body of known zero length: the headers still describe it, but nothing
            // goes on the wire. The content is still disposed below - it may hold a file handle.
            if (!ResponseSerializer.ShouldSendBody(request, response, headRequest))
            {
                return;
            }

            // Chunk-framed when the length is not known up front, since the header could not
            // declare one.
            if (content.Length is null && response.Mode != Connection.Upgrade)
            {
                var sink = new Http1ChunkedSink(writer);

                await content.WriteAsync(sink);

                sink.Finish();
            }
            else
            {
                await content.WriteAsync(new Http1Sink(writer));
            }
        }
        finally
        {
            if (content is IDisposable disposableContent)
            {
                disposableContent.Dispose();
            }
        }
    }
}
