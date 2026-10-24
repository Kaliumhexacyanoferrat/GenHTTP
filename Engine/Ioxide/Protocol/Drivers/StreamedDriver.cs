using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Protocol.Requests;
using GenHTTP.Engine.Ioxide.Protocol.Responses;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Protocol.Drivers;

/// <summary>
/// ioxide's HTTP/2 or HTTP/3 response writer, as <see cref="StreamedDriver"/> drives it. The two share
/// no type of their own, so each driver wraps its writer in one of these.
/// </summary>
internal interface IStreamedWriter
{

    /// <summary>The protocol's frame buffer, which the body is written into.</summary>
    IBufferWriter<byte> Body { get; }

    /// <summary>Pushes the staged frames out, and reports a peer that has gone.</summary>
    Func<ValueTask<FlushResult>> Flush { get; }

    /// <summary>Whether the stream has been ended already.</summary>
    bool IsCompleted { get; }

    /// <summary>Writes the response head.</summary>
    void WriteHead(StreamedResponseData head);

    /// <summary>Writes a head that carries a status and nothing else.</summary>
    void WriteHead(int status);

    /// <summary>Ends the stream.</summary>
    ValueTask CompleteAsync();

}

/// <summary>What the HTTP/2 and HTTP/3 drivers have in common: one stream through the handler chain.</summary>
internal static class StreamedDriver
{

    // A fault becomes a 500 while the head is still unwritten, and a reset once it is out. A client
    // that left mid-response is no fault at all.
    internal static async ValueTask DispatchAsync<TWriter>(IServer server, StreamedRequest request, TWriter writer,
        bool headRequest, string protocol) where TWriter : IStreamedWriter
    {
        // Whether the head is already on the wire. IsCompleted cannot answer this - it means the
        // stream has ended - so a fault AFTER the headers went out used to take the error path
        // anyway, where writing a second head throws. That second exception replaced the first, so
        // the only thing ever reported was the masking error and the real fault was never logged.
        var headersWritten = false;

        try
        {
            var response = await server.Handler.HandleAsync(request)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            writer.WriteHead(StreamedResponder.BuildHeaders(response, request));
            headersWritten = true;

            await StreamedResponder.WriteBodyAsync(response, writer.Body, writer.Flush, headRequest);

            await writer.CompleteAsync();
        }
        catch (ConnectionClosedException)
        {
            // The client reset or stopped the stream, or went away mid-response: nobody is left to
            // answer, and that is no fault.
        }
        catch (Exception e)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Drivers.StreamedDriver")
                  .LogError(e, "Failed to handle {Protocol} request", protocol);

            if (headersWritten)
            {
                // Too late for a 500, and ending the stream would pass the truncated body off as
                // whole. Rethrown, ioxide resets the stream (INTERNAL_ERROR on HTTP/2,
                // H3_INTERNAL_ERROR on HTTP/3), so the client sees it fail.
                throw;
            }

            if (!writer.IsCompleted)
            {
                writer.WriteHead(500);
            }

            await writer.CompleteAsync();
        }
    }

}
