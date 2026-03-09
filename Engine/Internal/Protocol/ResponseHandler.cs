using System.Buffers;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Internal.Protocol.Sinks;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class ResponseHandler : IResponseSink
{
    private static readonly ReadOnlyMemory<byte> ServerHeaderName = "Server"u8.ToArray();

    private readonly RegularSink _regularSink;

    private readonly ChunkedSink _chunkedSink;

    // todo: have a separate sink

    public IBufferWriter<byte> Writer => Context.Writer;

    public Stream Stream => Context.Stream;

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

    internal async ValueTask<bool> HandleAsync(IRequest? request, IResponse response, HttpProtocol version, bool keepAlive)
    {
        try
        {
            var raw = response.Raw;

            WriteStatus(request, raw);

            WriteHeader(raw, version, keepAlive);

            Context.Writer.Write("\r\n"u8);

            if (ShouldSendBody(request, response))
            {
                await WriteBodyAsync(raw);
            }

            var connected = Context.Connection.Connected;

            if (request != null)
            {
                Context.Server.Companion?.OnRequestHandled(request, response);
            }

            return connected;
        }
        catch (Exception)
        {
            // todo
            // Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, request?.Client.IPAddress, e);
            return false;
        }
    }

    private static bool ShouldSendBody(IRequest? request, IResponse response) => true; // todo
    /*(request == null || request.Method.KnownMethod != RequestMethod.Head) &&
    (
        response.ContentLength > 0 || response.Content?.Length > 0 ||
        response.ContentType is not null || response.ContentEncoding is not null ||
        response.Connection == Connection.Upgrade
    );*/

    private void WriteStatus(IRequest? request, IRawResponse response)
    {
        var writer = Context.Writer;

        writer.Write("HTTP/1.1 "u8);
        writer.Write(response.StatusCode);
        writer.Write(" "u8);
        writer.Write(response.StatusPhrase.Span);

        writer.Write("\r\n"u8);

        // todo
        // Output.Write((request?.ProtocolType == HttpProtocol.Http11) ? "HTTP/1.1 "u8 : "HTTP/1.0 "u8);
    }

    private void WriteHeader(IRawResponse response, HttpProtocol version, bool keepAlive)
    {
        var context = Context;

        var writer = context.Writer;

        if (!response.Headers.ContainsKey(ServerHeaderName))
        {
            writer.Write(ServerHeader.GetValue(context).Span);
        }

        writer.Write(DateHeader.GetValue().Span);

        var content = response.Content;

        if (content != null)
        {
            var length = content.Length;

            if (length != null)
            {
                writer.Write("Content-Length: "u8);
                writer.Write(length.Value);
                writer.Write("\r\n"u8);
            }
            else
            {
                writer.Write("Transfer-Encoding: chunked\r\n"u8);
            }
        }
        else
        {
            writer.Write("Content-Length: 0\r\n"u8);
        }

        /*if (response.Connection == Connection.Upgrade)
        {
            Output.Write("Connection: Upgrade\r\n"u8);
        }
        else if (version == HttpProtocol.Http10)
        {
            Output.Write(keepAlive ? "Connection: Keep-Alive\r\n"u8 : "Connection: Close\r\n"u8);
        }
        else if (!keepAlive)
        {
            // HTTP/1.1 connections are persistent by default so we do not need to send a Keep-Alive header
            Output.Write("Connection: Close\r\n"u8);
        }*/

        /*if (response.ContentType is not null)
        {
            Output.Write("Content-Type: "u8);
            Output.Write(response.ContentType.RawType);

            if (response.ContentType.Charset is not null)
            {
                Output.Write("; charset="u8);
                Output.Write(response.ContentType.Charset);
            }

            Output.Write("\r\n"u8);
        }

        if (response.ContentEncoding is not null)
        {
            Output.Write("Content-Encoding: "u8);
            Output.Write(response.ContentEncoding!);
            Output.Write("\r\n"u8);
        }

        if (response.ContentLength is not null)
        {
            Output.Write("Content-Length: "u8);
            Output.Write(response.ContentLength.Value);
            Output.Write("\r\n"u8);
        }
        else if (response.Connection != Connection.Upgrade)
        {
            Output.Write(response.Content is not null ? "Transfer-Encoding: chunked\r\n"u8 : "Content-Length: 0\r\n"u8);
        }

        if (response.Modified is not null)
        {
            Output.Write("Last-Modified: "u8);
            Output.Write(response.Modified.Value);
            Output.Write("\r\n"u8);
        }

        if (response.Expires is not null)
        {
            Output.Write("Expires: "u8);
            Output.Write(response.Expires.Value);
            Output.Write("\r\n"u8);
        }*/

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
    private ValueTask WriteBodyAsync(IRawResponse response)
    {
        var content = response.Content;

        if (content is null)
        {
            return ValueTask.CompletedTask;
        }

        var length = content.Length;

        if (length is null) // todo: && (response.Connection != Connection.Upgrade)
        {
            return WriteChunked(content);
        }

        _regularSink.Apply();

        return content.WriteAsync(_regularSink);

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
