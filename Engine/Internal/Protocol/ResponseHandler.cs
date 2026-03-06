using System.Buffers;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class ResponseHandler : IResponseSink
{

    #region Get-/Setters

    private IServer Server { get; }

    private Socket Socket { get; }

    private NetworkConfiguration Configuration { get; }

    public IBufferWriter<byte> Writer { get; }

    public Stream Stream { get; }

    #endregion

    #region Initialization

    internal ResponseHandler(IServer server, Socket socket, IBufferWriter<byte> writer, Stream stream, NetworkConfiguration configuration)
    {
        Server = server;
        Socket = socket;

        Writer = writer;
        Stream = stream;

        Configuration = configuration;
    }

    #endregion

    #region Functionality

    internal bool Handle(IRequest? request, IResponse response, HttpProtocol version, bool keepAlive)
    {
        try
        {
            var raw = response.Raw;

            WriteStatus(request, raw);

            WriteHeader(raw, version, keepAlive);

            Writer.Write("\r\n"u8);

            if (ShouldSendBody(request, response))
            {
                WriteBody(raw);
            }

            var connected = Socket.Connected;

            if (request != null)
            {
                Server.Companion?.OnRequestHandled(request, response);
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
        Writer.Write("HTTP/1.1 "u8);
        Writer.Write(response.StatusCode);
        Writer.Write(" "u8);
        Writer.Write(response.StatusPhrase.Span);

        Writer.Write("\r\n"u8);

        // todo
        // Output.Write((request?.ProtocolType == HttpProtocol.Http11) ? "HTTP/1.1 "u8 : "HTTP/1.0 "u8);
    }

    private void WriteHeader(IRawResponse response, HttpProtocol version, bool keepAlive)
    {
        /*if (response.Headers.TryGetValue("Server", out var server))
        {
            Output.Write("Server: "u8);
            Output.Write(server);
            Output.Write("\r\n"u8);
        }
        else
        {*/
            Writer.Write("Server: GenHTTP/"u8);
            Writer.Write(Server.Version);
            Writer.Write("\r\n"u8);
        //}

        Writer.Write("Date: "u8);
        Writer.Write(DateHeader.GetValue());
        Writer.Write("\r\n"u8);

        var content = response.Content;

        if (content != null)
        {
            var length = content.Length;

            if (length != null)
            {
                Writer.Write("Content-Length: "u8);
                Writer.Write(length.Value);
                Writer.Write("\r\n"u8);
            }
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

            Writer.Write(header.Key.Span);
            Writer.Write(": "u8);
            Writer.Write(header.Value.Span);
            Writer.Write("\r\n"u8);
        }

        /*if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                WriteCookie(cookie.Value);
            }
        }*/
    }

    private void WriteBody(IRawResponse response)
    {
        if (response.Content is not null)
        {
            response.Content.WriteAsync(this);
            /*if (response.ContentLength is null && (response.Connection != Connection.Upgrade))
            {
                await using var chunked = new ChunkedStream(Output);

                await response.Content.WriteAsync(chunked, Configuration.TransferBufferSize);

                chunked.Finish();
            }
            else
            {
                await response.Content.WriteAsync(Output, Configuration.TransferBufferSize);
            }*/
        }
    }

    #endregion

    #region Helpers

    private void WriteCookie(Cookie cookie)
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
    }

    #endregion

}
