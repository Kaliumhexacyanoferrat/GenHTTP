using System.Text;

using GenHTTP.Api.Protocol;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Writes an <see cref="IResponse"/> onto Kestrel's <see cref="IHttpResponseFeature"/>/
/// <see cref="IHttpResponseBodyFeature"/>. Kestrel owns wire framing itself, so this only
/// ever sets feature state and writes body bytes.
/// </summary>
public sealed class ResponseWriter(ClientContext context)
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
                // Must happen here, synchronously with the headers set above: UpgradeAsync()
                // flushes the response feature and hands back the raw duplex stream that
                // IRequest.Upgrade() (a synchronous API) needs to already have available.
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

        var isUpgrade = response.Mode == Connection.Upgrade;

        if (content != null)
        {
            if (content.Type is { } type)
            {
                headers.ContentType = type.ToString();
            }

            // An upgrade response has no HTTP body - Kestrel silently rejects it if
            // Content-Length is set, so it's left unset (chunked/DATA framing otherwise).
            if (!isUpgrade && content.Length is { } length)
            {
                headers.ContentLength = (long)length;
            }

            if (content.Encoding is { } encoding)
            {
                headers.ContentEncoding = Encoding.ASCII.GetString(encoding.Span);
            }
        }
        else if (!isUpgrade)
        {
            headers.ContentLength = 0;
        }

        if (response.Mode == Connection.Close)
        {
            headers.Connection = "close";
        }

        var responseHeaders = response.Headers;

        // Append, not assign - a response can carry several headers with the same name
        // (e.g. multiple Set-Cookie entries), and IHeaderDictionary's indexer would
        // silently replace earlier ones instead of keeping both.
        for (var i = 0; i < responseHeaders.Count; i++)
        {
            var pair = responseHeaders.GetMemoryEntry(i);

            headers.Append(Encoding.ASCII.GetString(pair.Key.Span), Encoding.ASCII.GetString(pair.Value.Span));
        }

        if (!response.Headers.ContainsKey(KnownHeaders.Server))
        {
            headers.Server = $"GenHTTP-Kestrel/{context.Server.Version}";
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
