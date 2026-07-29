using System.Text;

using GenHTTP.Api.Protocol;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Kestrel.Context;

/// <summary>
/// Writes a <see cref="IResponse"/> onto Kestrel's <see cref="IHttpResponseFeature"/>/
/// <see cref="IHttpResponseBodyFeature"/>.
/// </summary>
/// <remarks>
/// Replaces <c>Engine.Shared.Types.ResponseHandler</c> for this engine: that type hand-writes
/// the status line, headers and chunked transfer-encoding as raw bytes onto a PipeWriter,
/// which only makes sense for engines that own the wire format themselves (Internal/Ioxide).
/// Kestrel owns framing for HTTP/1.1, h2 and h3 alike, so we only ever set feature state and
/// write body bytes - never wire bytes.
/// </remarks>
internal sealed class ResponseWriter(ClientContext context)
{
    private readonly ResponseSink _sink = new(context);

    #region Functionality

    public async ValueTask HandleAsync(Request request, IResponse response, bool headRequest)
    {
        try
        {
            var features = request.Features;

            var responseFeature = features.GetRequiredFeature<IHttpResponseFeature>();

            responseFeature.StatusCode = (int)response.Status;

            WriteHeaders(responseFeature, response);

            if (response.Mode == Connection.Upgrade)
            {
                // Sending the upgrade response (e.g. 101 Switching Protocols) has to happen
                // here, synchronously with the headers set above - IHttpUpgradeFeature.
                // UpgradeAsync() flushes whatever is currently set on the response feature
                // and hands back the raw duplex connection stream. IRequest.Upgrade() (used
                // by e.g. the websocket module, invoked further down inside WriteBodyAsync)
                // is a synchronous API, so the stream has to already be available by then.
                var upgradeFeature = features.GetRequiredFeature<IHttpUpgradeFeature>();

                var stream = await upgradeFeature.UpgradeAsync();

                request.SetUpgraded(stream);
            }

            if (ShouldSendBody(response, headRequest))
            {
                await WriteBodyAsync(response);
            }

            // Writes above only fill our buffering PipeWriter (see ClientContext.Writer) -
            // nothing else pushes those bytes to Kestrel's response body feature.
            await context.Writer.FlushAsync();
        }
        catch (Exception e)
        {
            context.Server.Logging.CreateLogger<ResponseWriter>().LogWarning(e, "Failed to write response to client");
        }
    }

    private static bool ShouldSendBody(IResponse response, bool headRequest)
    {
        if (headRequest)
        {
            return false;
        }

        var content = response.Content;

        if (content != null)
        {
            return (content.Length ?? 1) > 0;
        }

        return false;
    }

    private void WriteHeaders(IHttpResponseFeature responseFeature, IResponse response)
    {
        var headers = responseFeature.Headers;

        var content = response.Content;

        if (content != null)
        {
            if (content.Type is { } type)
            {
                headers.ContentType = type.ToString();
            }

            if (content.Length is { } length)
            {
                headers.ContentLength = (long)length;
            }

            // else: leave Content-Length unset - Kestrel applies chunked framing (HTTP/1.1)
            // or DATA frames (h2/h3) itself, there is no "Transfer-Encoding: chunked" to write.

            if (content.Encoding is { } encoding)
            {
                headers.ContentEncoding = Encoding.ASCII.GetString(encoding.Span);
            }
        }
        else
        {
            headers.ContentLength = 0;
        }

        if (response.Mode == Connection.Close)
        {
            headers.Connection = "close";
        }

        var responseHeaders = response.Headers;

        for (var i = 0; i < responseHeaders.Count; i++)
        {
            var pair = responseHeaders.GetMemoryEntry(i);

            headers[Encoding.ASCII.GetString(pair.Key.Span)] = Encoding.ASCII.GetString(pair.Value.Span);
        }

        if (!response.Headers.ContainsKey(KnownHeaders.Server))
        {
            headers.Server = $"GenHTTP/{context.Server.Version}";
        }

        // Date is intentionally left to Kestrel, which sets it itself for HTTP/1.x.
    }

    private async ValueTask WriteBodyAsync(IResponse response)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        _sink.Apply();

        await content.WriteAsync(_sink);

        if (content is IDisposable disposableContent)
        {
            disposableContent.Dispose();
        }
    }

    #endregion

}
