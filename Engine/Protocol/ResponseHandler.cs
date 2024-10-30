using System.Buffers;
using System.Net.Sockets;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Infrastructure;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol;

internal sealed class ResponseHandler
{
    private const string ServerHeader = "Server";

    private const string NL = "\r\n";

    private static readonly Encoding Ascii = Encoding.ASCII;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    #region Get-/Setters

    private IServer Server { get; }

    private Socket Socket { get; }

    private Stream OutputStream { get; }

    private NetworkConfiguration Configuration { get; }

    #endregion

    #region Initialization

    internal ResponseHandler(IServer server, Socket socket, Stream outputStream, NetworkConfiguration configuration)
    {
        Server = server;
        Socket = socket;

        OutputStream = outputStream;

        Configuration = configuration;
    }

    #endregion

    #region Functionality

    internal async ValueTask<bool> Handle(IRequest? request, IResponse response, bool keepAlive, bool dataRemaining)
    {
        try
        {
            await WriteStatus(request, response);

            await WriteHeader(response, keepAlive);

            await Write(NL);

            if (ShouldSendBody(request, response))
            {
                await WriteBody(response);
            }

            var connected = Socket.Connected;

            // flush if the client waits for this response
            // otherwise save flushes for improved performance when pipelining
            if (!dataRemaining && connected)
            {
                await OutputStream.FlushAsync();
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

    private ValueTask WriteStatus(IRequest? request, IResponse response)
    {
        var version = request?.ProtocolType == HttpProtocol.Http11 ? "1.1" : "1.0";

        return Write("HTTP/", version, " ", NumberStringCache.Convert(response.Status.RawStatus), " ", response.Status.Phrase, NL);
    }

    private async ValueTask WriteHeader(IResponse response, bool keepAlive)
    {
        if (response.Headers.TryGetValue(ServerHeader, out var server))
        {
            await WriteHeaderLine(ServerHeader, server);
        }
        else
        {
            await Write("Server: GenHTTP/", Server.Version, NL);
        }

        await WriteHeaderLine("Date", DateHeader.GetValue());

        await WriteHeaderLine("Connection", keepAlive ? "Keep-Alive" : "Close");

        if (response.ContentType is not null)
        {
            if (response.ContentType.Charset is not null)
            {
                await Write("Content-Type: ", response.ContentType.RawType, "; charset=", response.ContentType.Charset, NL);
            }
            else
            {
                await WriteHeaderLine("Content-Type", response.ContentType.RawType);
            }
        }

        if (response.ContentEncoding is not null)
        {
            await WriteHeaderLine("Content-Encoding", response.ContentEncoding!);
        }

        if (response.ContentLength is not null)
        {
            await WriteHeaderLine("Content-Length", NumberStringCache.Convert(response.ContentLength.Value));
        }
        else
        {
            if (response.Content is not null)
            {
                await WriteHeaderLine("Transfer-Encoding", "chunked");
            }
            else
            {
                await WriteHeaderLine("Content-Length", "0");
            }
        }

        if (response.Modified is not null)
        {
            await WriteHeaderLine("Last-Modified", (DateTime)response.Modified);
        }

        if (response.Expires is not null)
        {
            await WriteHeaderLine("Expires", (DateTime)response.Expires);
        }

        foreach (var header in response.Headers)
        {
            if (!header.Key.Equals(ServerHeader, StringComparison.OrdinalIgnoreCase))
            {
                await WriteHeaderLine(header.Key, header.Value);
            }
        }

        if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                await WriteCookie(cookie.Value);
            }
        }
    }

    private async ValueTask WriteBody(IResponse response)
    {
        if (response.Content is not null)
        {
            if (response.ContentLength is null)
            {
                await using var chunked = new ChunkedStream(OutputStream);

                await response.Content.WriteAsync(chunked, Configuration.TransferBufferSize);

                await chunked.FinishAsync();
            }
            else
            {
                await response.Content.WriteAsync(OutputStream, Configuration.TransferBufferSize);
            }
        }
    }

    #endregion

    #region Helpers

    private ValueTask WriteHeaderLine(string key, string value) => Write(key, ": ", value, NL);

    private ValueTask WriteHeaderLine(string key, DateTime value) => WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));

    private async ValueTask WriteCookie(Cookie cookie)
    {
        await Write("Set-Cookie: ", cookie.Name, "=", cookie.Value);

        if (cookie.MaxAge is not null)
        {
            await Write("; Max-Age=", NumberStringCache.Convert(cookie.MaxAge.Value));
        }

        await Write("; Path=/", NL);
    }

    /// <summary>
    /// Writes the given parts to the output stream.
    /// </summary>
    /// <remarks>
    /// Reduces the number of writes to the output stream by collecting
    /// data to be written. Cannot use params keyword because it allocates
    /// an array.
    /// </remarks>
    private async ValueTask Write(string part1, string? part2 = null, string? part3 = null,
        string? part4 = null, string? part5 = null, string? part6 = null, string? part7 = null)
    {
        var length = part1.Length + (part2?.Length ?? 0) + (part3?.Length ?? 0) + (part4?.Length ?? 0)
            + (part5?.Length ?? 0) + (part6?.Length ?? 0) + (part7?.Length ?? 0);

        var buffer = Pool.Rent(length);

        try
        {
            var index = Ascii.GetBytes(part1, 0, part1.Length, buffer, 0);

            if (part2 is not null)
            {
                index += Ascii.GetBytes(part2, 0, part2.Length, buffer, index);
            }

            if (part3 is not null)
            {
                index += Ascii.GetBytes(part3, 0, part3.Length, buffer, index);
            }

            if (part4 is not null)
            {
                index += Ascii.GetBytes(part4, 0, part4.Length, buffer, index);
            }

            if (part5 is not null)
            {
                index += Ascii.GetBytes(part5, 0, part5.Length, buffer, index);
            }

            if (part6 is not null)
            {
                index += Ascii.GetBytes(part6, 0, part6.Length, buffer, index);
            }

            if (part7 is not null)
            {
                Ascii.GetBytes(part7, 0, part7.Length, buffer, index);
            }

            await OutputStream.WriteAsync(buffer.AsMemory(0, length));
        }
        finally
        {
            Pool.Return(buffer);
        }
    }

    #endregion

}
