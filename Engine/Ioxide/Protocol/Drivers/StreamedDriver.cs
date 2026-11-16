using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Protocol.Requests;
using GenHTTP.Engine.Ioxide.Protocol.Responses;

using ioxide;

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

    /// <summary>Resets the stream, so the client sees the response fail.</summary>
    ValueTask ResetStreamAsync();

}

/// <summary>What the HTTP/2 and HTTP/3 drivers have in common: one stream through the handler chain.</summary>
internal static class StreamedDriver
{
    private const string LoggerCategory = "GenHTTP.Engine.Ioxide.Protocol.Drivers.StreamedDriver";

    // ioxide's OnHandlerFault, so what it still catches itself goes to the server's logging instead of
    // stderr: a fault in the error path below, or the stack failing to finish a response once its
    // handler has returned.
    internal static Action<TRequest, Exception> ReportFaults<TRequest>(ILoggerFactory logging, string protocol)
    {
        var logger = logging.CreateLogger(LoggerCategory);

        return (_, e) => logger.LogError(e, "Failed to complete {Protocol} request", protocol);
    }

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

        // Taken here, on the reactor: the handler may come back on another thread.
        var reactor = Reactor.Current;

        try
        {
            var response = await server.Handler.HandleAsync(request)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            // The response, with the thread-statics it reads, is written on the reactor.
            await new ReactorSwitch(reactor);

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
            server.Logging.CreateLogger(LoggerCategory)
                  .LogError(e, "Failed to handle {Protocol} request", protocol);

            if (headersWritten)
            {
                // Too late for a 500, and ending the stream would pass the truncated body off as
                // whole. Rethrown, ioxide would reset it as well, but report the fault a second time.
                await writer.ResetStreamAsync();
                return;
            }

            if (!writer.IsCompleted)
            {
                writer.WriteHead(500);
            }

            await writer.CompleteAsync();
        }
    }

}
