using GenHTTP.Engine.Internal.Protocol.Parser.Conversion;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Protocol.Parser;

/// <summary>
/// Reads the next HTTP request to be handled by the server from
/// the client connection.
/// </summary>
/// <remarks>
/// Be aware that this code path is heavily optimized for low
/// memory allocations. Changes to this class should allocate
/// as few memory as possible to avoid the performance of
/// the server from being impacted in a negative manner.
/// </remarks>
internal sealed class RequestParser
{

    #region Initialization

    internal RequestParser(NetworkConfiguration configuration, Request request)
    {
        Configuration = configuration;
        Request = request;

        Scanner = new RequestScanner();
    }

    #endregion

    #region Get-/Setters

    private NetworkConfiguration Configuration { get; }

    private RequestScanner Scanner { get; }

    private Request Request { get; }

    #endregion

    #region Functionality

    internal async ValueTask<bool> TryParseAsync(RequestBuffer buffer)
    {
        if (!await Type(buffer))
        {
            if (!buffer.Data.IsEmpty)
            {
                throw new ProtocolException("Unable to read HTTP verb from request line.");
            }

            return false;
        }

        await Path(buffer);

        await Protocol(buffer);

        await Headers(buffer);

        await Body(buffer);

        return true;
    }

    private async ValueTask<bool> Type(RequestBuffer buffer)
    {
        Scanner.Mode = ScannerMode.Words;

        if (await Scanner.Next(buffer, RequestToken.Word, true))
        {
            Request.SetMethod(MethodConverter.ToRequestMethod(Scanner.Value));
            return true;
        }

        return false;
    }

    private async ValueTask Path(RequestBuffer buffer)
    {
        Scanner.Mode = ScannerMode.Path;

        var token = await Scanner.Next(buffer);

        // path
        if (token == RequestToken.Path)
        {
            Request.SetPath(PathConverter.ToPath(Scanner.Value));
        }
        else if (token == RequestToken.PathWithQuery)
        {
            Request.SetPath(PathConverter.ToPath(Scanner.Value));

            // query
            Scanner.Mode = ScannerMode.Words;

            if (await Scanner.Next(buffer, RequestToken.Word, includeWhitespace: true))
            {
                var query = QueryConverter.ToQuery(Scanner.Value);

                if (query != null)
                {
                    Request.SetQuery(query);
                }
            }
        }
        else
        {
            throw new ProtocolException($"Unexpected token while parsing path: {token}");
        }
    }

    private async ValueTask Protocol(RequestBuffer buffer)
    {
        Scanner.Mode = ScannerMode.Words;

        if (await Scanner.Next(buffer, RequestToken.Word))
        {
            Request.SetProtocol(ProtocolConverter.ToProtocol(Scanner.Value));
        }
    }

    private async ValueTask Headers(RequestBuffer buffer)
    {
        Scanner.Mode = ScannerMode.HeaderKey;

        while (await Scanner.Next(buffer) == RequestToken.Word)
        {
            var key = HeaderConverter.ToKey(Scanner.Value);

            Scanner.Mode = ScannerMode.HeaderValue;

            if (await Scanner.Next(buffer, RequestToken.Word))
            {
                Request.SetHeader(key, HeaderConverter.ToValue(Scanner.Value));
            }

            Scanner.Mode = ScannerMode.HeaderKey;
        }
    }

    private async ValueTask Body(RequestBuffer buffer)
    {
        if (Request.Headers.TryGetValue("Content-Length", out var bodyLength))
        {
            if (long.TryParse(bodyLength, out var length))
            {
                if (length > 0)
                {
                    var parser = new RequestContentParser(length, Configuration);

                    Request.SetContent(await parser.GetBody(buffer));
                }
            }
            else
            {
                throw new ProtocolException("Content-Length header is expected to be a numeric value");
            }
        }
        else if (Request.Headers.TryGetValue("Transfer-Encoding", out var mode))
        {
            if (mode == "chunked")
            {
                var parser = new ChunkedContentParser(Configuration);

                Request.SetContent(await parser.GetBody(buffer));
            }
        }
    }

    #endregion

}
