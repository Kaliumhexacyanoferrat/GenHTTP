﻿using GenHTTP.Api.Protocol;

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
    private RequestBuilder? _Builder;

    #region Initialization

    internal RequestParser(NetworkConfiguration configuration)
    {
        Configuration = configuration;
        Scanner = new RequestScanner();
    }

    #endregion

    #region Get-/Setters

    private NetworkConfiguration Configuration { get; }

    private RequestBuilder Request => _Builder ??= new RequestBuilder();

    private RequestScanner Scanner { get; }

    #endregion

    #region Functionality

    internal async ValueTask<RequestBuilder?> TryParseAsync(RequestBuffer buffer)
    {
        if (!await Type(buffer))
        {
            if (!buffer.Data.IsEmpty)
            {
                throw new ProtocolException("Unable to read HTTP verb from request line.");
            }

            return null;
        }

        await Path(buffer);

        await Protocol(buffer);

        await Headers(buffer);

        await Body(buffer);

        var result = Request;
        _Builder = null;

        return result;
    }

    private async ValueTask<bool> Type(RequestBuffer buffer)
    {
        Scanner.Mode = ScannerMode.Words;

        if (await Scanner.Next(buffer, RequestToken.Word, true))
        {
            Request.Type(MethodConverter.ToRequestMethod(Scanner.Value));
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
            Request.Path(PathConverter.ToPath(Scanner.Value));
        }
        else if (token == RequestToken.PathWithQuery)
        {
            Request.Path(PathConverter.ToPath(Scanner.Value));

            // query
            Scanner.Mode = ScannerMode.Words;

            if (await Scanner.Next(buffer, RequestToken.Word, includeWhitespace: true))
            {
                var query = QueryConverter.ToQuery(Scanner.Value);

                if (query != null)
                {
                    Request.Query(query);
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
            Request.Protocol(ProtocolConverter.ToProtocol(Scanner.Value));
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
                Request.Header(key, HeaderConverter.ToValue(Scanner.Value));
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

                    Request.Content(await parser.GetBody(buffer));
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

                Request.Content(await parser.GetBody(buffer));
            }
        }
    }

    #endregion

}
