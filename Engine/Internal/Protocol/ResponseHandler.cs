using System.Buffers;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Internal.Protocol.Sinks;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class ResponseHandler
{

    private readonly RegularSink _regularSink;

    private readonly ChunkedSink _chunkedSink;

    private ClientContext Context { get; }

    #region Initialization

    internal ResponseHandler(ClientContext context)
    {
        Context = context;

        _regularSink = new(Context);
        _chunkedSink = new(Context);
    }

    #endregion

    #region Functionality

    internal async ValueTask<bool> HandleAsync(IRequest? request, IResponse response, HttpProtocol version, bool keepAlive, bool headRequest)
    {
        try
        {
            var writer = Context.Writer;

            writer.Write(StatusLine.Get(response.Status));

            WriteHeader(response, version, keepAlive);

            writer.Write("\r\n"u8);

            if (ShouldSendBody(request, response, headRequest))
            {
                await WriteBodyAsync(response);
            }

            if (request != null)
            {
                Context.Server.Companion?.OnRequestHandled(request, response);
            }

            return true;
        }
        catch (Exception)
        {
            // todo
            // Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, request?.Client.IPAddress, e);
            return false;
        }
    }

    private static bool ShouldSendBody(IRequest? request, IResponse response, bool headRequest)
    {
        if (request == null)
        {
            return true;
        }

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

    private void WriteHeader(IResponse response, HttpProtocol version, bool keepAlive)
    {
        var context = Context;

        var writer = context.Writer;

        var isUpgrade = response.Mode == Connection.Upgrade;

        if (!response.Headers.ContainsKey(KnownHeaders.Server))
        {
            writer.Write(ServerHeader.GetValue(context).Span);
        }

        if (!response.Headers.ContainsKey(KnownHeaders.Date))
        {
            writer.Write(DateHeader.GetValue().Span);
        }

        if (isUpgrade)
        {
            writer.Write("Connection: Upgrade\r\n"u8);
        }
        else if (version == HttpProtocol.Http10)
        {
            writer.Write(keepAlive ? "Connection: Keep-Alive\r\n"u8 : "Connection: Close\r\n"u8);
        }
        else if (!keepAlive)
        {
            // HTTP/1.1 connections are persistent by default so we do not need to send a Keep-Alive header
            writer.Write("Connection: Close\r\n"u8);
        }

        var content = response.Content;

        if (content != null)
        {
            var type = content.Type;

            if (type != null)
            {
                writer.Write("Content-Type: "u8);
                writer.Write(type.Value.Value.Span);
                writer.Write("\r\n"u8);
            }

            var length = content.Length;

            if (length != null)
            {
                writer.Write("Content-Length: "u8);
                writer.Write(length.Value);
                writer.Write("\r\n"u8);
            }
            else if (!isUpgrade)
            {
                writer.Write("Transfer-Encoding: chunked\r\n"u8);
            }

            var encoding = content.Encoding;

            if (encoding != null)
            {
                writer.Write("Content-Encoding: "u8);
                writer.Write(encoding.Value.Span);
                writer.Write("\r\n"u8);
            }
        }
        else
        {
            writer.Write("Content-Length: 0\r\n"u8);
        }

        var headers = response.Headers;

        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];

            writer.Write(header.Key.Span);
            writer.Write(": "u8);
            writer.Write(header.Value.Span);
            writer.Write("\r\n"u8);
        }

        /*if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                WriteCookie(cookie.Value);
            }
        }*/
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask WriteBodyAsync(IResponse response)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        var length = content.Length;

        if (length is null && response.Mode != Connection.Upgrade)
        {
            await WriteChunked(content);
        }
        else
        {
            _regularSink.Apply();
            await content.WriteAsync(_regularSink);
        }

        if (content is IDisposable disposableContent)
        {
            disposableContent.Dispose();
        }
    }

    private async ValueTask WriteChunked(IResponseContent content)
    {
        _chunkedSink.Apply();

        await content.WriteAsync(_chunkedSink);

        _chunkedSink.Finish();
    }

    #endregion

    #region Helpers

    /*private void WriteCookie(Cookie cookie)
    {
        Writer.Write("Set-Cookie: "u8);
        Writer.Write(cookie.Name);
        Writer.Write("="u8);
        Writer.Write(cookie.Value);

        if (cookie.MaxAge is not null)
        {
            Writer.Write("; Max-Age="u8);
            Writer.Write(cookie.MaxAge.Value);
        }

        Writer.Write("; Path=/\r\n"u8);
    }*/

    #endregion

}
