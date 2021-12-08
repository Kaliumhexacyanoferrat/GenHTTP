using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Infrastructure.Configuration;
using GenHTTP.Engine.Utilities;

using PooledAwait;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class ResponseHandler
    {
        private const string SERVER_HEADER = "Server";

        private static readonly string NL = "\r\n";

        private static readonly Encoding ASCII = Encoding.ASCII;

        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        private IServer Server { get; }

        private Stream OutputStream { get; }

        private Socket Socket { get; }

        internal NetworkConfiguration Configuration { get; }

        #endregion

        #region Initialization

        internal ResponseHandler(IServer server, Stream outputstream, Socket socket, NetworkConfiguration configuration)
        {
            Server = server;

            OutputStream = outputstream;
            Socket = socket;

            Configuration = configuration;
        }

        #endregion

        #region Functionality

        internal async ValueTask<bool> Handle(IRequest request, IResponse response, bool keepAlive)
        {
            try
            {
                await WriteStatus(request, response).ConfigureAwait(false);

                await WriteHeader(response, keepAlive);

                await Write(NL);

                if (ShouldSendBody(request, response))
                {
                    await WriteBody(response);
                }

                Socket.NoDelay = true;

                await OutputStream.FlushAsync();

                Socket.NoDelay = false;

                Server.Companion?.OnRequestHandled(request, response);

                return true;
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                return false;
            }
        }

        private static bool ShouldSendBody(IRequest request, IResponse response)
        {
            return (request.Method.KnownMethod != RequestMethod.HEAD) &&
                   (
                     (response.ContentLength > 0) || (response.Content?.Length > 0) ||
                     (response.ContentType is not null) || (response.ContentEncoding is not null)
                   );
        }

        private async ValueTask WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";

            await Write("HTTP/").ConfigureAwait(false);
            await Write(version);

            await Write(" ");

            await Write(ConvertToString(response.Status.RawStatus));

            await Write(" ");

            await Write(response.Status.Phrase);

            await Write(NL);
        }

        private async PooledValueTask WriteHeader(IResponse response, bool keepAlive)
        {
            if (response.Headers.TryGetValue(SERVER_HEADER, out var server))
            {
                await WriteHeaderLine(SERVER_HEADER, server).ConfigureAwait(false);
            }
            else
            {
                await Write("Server: GenHTTP/").ConfigureAwait(false);
                await Write(Server.Version);
                await Write(NL);
            }

            await WriteHeaderLine("Date", DateHeader.GetValue());

            await WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (response.ContentType is not null)
            {
                if (response.ContentType.Charset is not null)
                {
                    await Write("Content-Type: ");
                    await Write(response.ContentType.RawType);
                    await Write("; charset=");
                    await Write(response.ContentType.Charset);
                    await Write(NL);
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
                await WriteHeaderLine("Content-Length", ConvertToString((ulong)response.ContentLength));
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
                if (!header.Key.Equals(SERVER_HEADER, StringComparison.OrdinalIgnoreCase))
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

        private async PooledValueTask WriteBody(IResponse response)
        {
            if (response.Content is not null)
            {
                if (response.ContentLength is null)
                {
                    using var chunked = new ChunkedStream(OutputStream);

                    await response.Content.WriteAsync(chunked, Configuration.TransferBufferSize).ConfigureAwait(false);

                    await chunked.FinishAsync();
                }
                else
                {
                    await response.Content.WriteAsync(OutputStream, Configuration.TransferBufferSize).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private async ValueTask WriteHeaderLine(string key, string value)
        {
            await Write(key).ConfigureAwait(false);
            await Write(": ");
            await Write(value);
            await Write(NL);
        }

        private ValueTask WriteHeaderLine(string key, DateTime value)
        {
            return WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));
        }

        private async ValueTask WriteCookie(Cookie cookie)
        {
            await Write("Set-Cookie: ").ConfigureAwait(false);

            await Write(cookie.Name);
            await Write("=");
            await Write(cookie.Value);

            if (cookie.MaxAge is not null)
            {
                await Write("; Max-Age=");
                await Write(ConvertToString(cookie.MaxAge.Value));
            }

            await Write("; Path=/");

            await Write(NL);
        }

        private async PooledValueTask Write(string text)
        {
            var length = text.Length;

            var buffer = POOL.Rent(length);

            try
            {
                ASCII.GetBytes(text, 0, length, buffer, 0);

                await OutputStream.WriteAsync(buffer.AsMemory(0, length)).ConfigureAwait(false);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        private static string ConvertToString(int number) => NumberStringCache.Convert(number);

        private static string ConvertToString(ulong number)
        {
            if (number < 1024)
            {
                return NumberStringCache.Convert((int)number);
            }

            return $"{number}";
        }

        #endregion

    }

}
