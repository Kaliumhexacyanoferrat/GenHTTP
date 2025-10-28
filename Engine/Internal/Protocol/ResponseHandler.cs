using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class ResponseHandler
{

    #region Get-/Setters

    private IServer Server { get; }

    private Socket Socket { get; }

    private Stream Output { get; }

    private NetworkConfiguration Configuration { get; }

    #endregion

    #region Initialization

    internal ResponseHandler(IServer server, Socket socket, Stream output, NetworkConfiguration configuration)
    {
        Server = server;
        Socket = socket;

        Output = output;

        Configuration = configuration;
    }

    #endregion

    #region Functionality

    internal async ValueTask<bool> Handle(IRequest? request, IResponse response, HttpProtocol version, bool keepAlive, bool dataRemaining)
    {
        try
        {
            WriteStatus(request, response);

            WriteHeader(response, version, keepAlive);

            Output.Write("\r\n"u8);

            if (ShouldSendBody(request, response))
            {
                await WriteBody(response);
            }

            var connected = Socket.Connected;

            // flush if the client waits for this response
            // otherwise save flushes for improved performance when pipelining
            if (!dataRemaining && connected)
            {
                await Output.FlushAsync();
            }

            if (request != null)
            {
                Server.Companion?.OnRequestHandled(request, response);
            }

            return connected;
        }
        catch (Exception e)
        {
            Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, request?.Client.IPAddress, e);
            return false;
        }
    }

    private static bool ShouldSendBody(IRequest? request, IResponse response) => (request == null || request.Method.KnownMethod != RequestMethod.Head) &&
    (
        response.ContentLength > 0 || response.Content?.Length > 0 ||
        response.ContentType is not null || response.ContentEncoding is not null
    );

    private void WriteStatus(IRequest? request, IResponse response)
    {
        Output.Write("HTTP/"u8);
        Output.Write((request?.ProtocolType == HttpProtocol.Http11) ? "1.1"u8 : "1.0"u8);
        Output.Write(" "u8);
        Output.Write(response.Status.RawStatus);
        Output.Write(" "u8);

        Output.Write(response.Status.Phrase);

        Output.Write("\r\n"u8);
    }

    private void WriteHeader(IResponse response, HttpProtocol version, bool keepAlive)
    {
        if (response.Headers.TryGetValue("Server", out var server))
        {
            Output.Write("Server: "u8);
            Output.Write(server);
            Output.Write("\r\n"u8);
        }
        else
        {
            Output.Write("Server: "u8);
            Output.Write(Server.Version);
            Output.Write("\r\n"u8);
        }

        Output.Write("Date: "u8);
        Output.Write(DateHeader.GetValue());
        Output.Write("\r\n"u8);

        if (version == HttpProtocol.Http10)
        {
            Output.Write(keepAlive ? "Connection: Keep-Alive\r\n"u8 : "Connection: Close\r\n"u8);
        }
        else if (!keepAlive)
        {
            // HTTP/1.1 connections are persistent by default so we do not need to send a Keep-Alive header
            Output.Write("Connection: Close\r\n"u8);
        }

        if (response.ContentType is not null)
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
        else
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
        }

        var serverSpan = "Server".AsSpan();

        foreach (var header in response.Headers)
        {
            var keySpan = header.Key.AsSpan();

            if (!keySpan.Equals(serverSpan, StringComparison.OrdinalIgnoreCase))
            {
                Output.Write(header.Key);
                Output.Write(": "u8);
                Output.Write(header.Value);
                Output.Write("\r\n"u8);
            }
        }

        if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                WriteCookie(cookie.Value);
            }
        }
    }

    private async ValueTask WriteBody(IResponse response)
    {
        if (response.Content is not null)
        {
            if (response.ContentLength is null)
            {
                await using var chunked = new ChunkedStream(Output);

                await response.Content.WriteAsync(chunked, Configuration.TransferBufferSize);

                chunked.Finish();
            }
            else
            {
                await response.Content.WriteAsync(Output, Configuration.TransferBufferSize);
            }
        }
    }

    #endregion

    #region Helpers

    private void WriteCookie(Cookie cookie)
    {
        Output.Write("Set-Cookie: "u8);
        Output.Write(cookie.Name);
        Output.Write("="u8);
        Output.Write(cookie.Value);

        if (cookie.MaxAge is not null)
        {
            Output.Write("; Max-Age="u8);
            Output.Write(cookie.MaxAge.Value);
        }

        Output.Write("; Path=/\r\n"u8);
    }

    #endregion

}
