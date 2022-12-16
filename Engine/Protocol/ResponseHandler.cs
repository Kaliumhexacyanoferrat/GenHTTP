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

        private ValueTask WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";

            return Write("HTTP/", version, " ", NumberStringCache.Convert(response.Status.RawStatus), " ", response.Status.Phrase, NL);
        }

        private async ValueTask WriteHeader(IResponse response, bool keepAlive)
        {
            if (response.Headers.TryGetValue(SERVER_HEADER, out var server))
            {
                await WriteHeaderLine(SERVER_HEADER, server).ConfigureAwait(false);
            }
            else
            {
                await Write("Server: GenHTTP/", Server.Version, NL).ConfigureAwait(false);
            }

            await WriteHeaderLine("Date", DateHeader.GetValue());

            await WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close");

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

        private async ValueTask WriteBody(IResponse response)
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

        private ValueTask WriteHeaderLine(string key, string value) => Write(key, ": ", value, NL);

        private ValueTask WriteHeaderLine(string key, DateTime value) => WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));

        private async ValueTask WriteCookie(Cookie cookie)
        {
            await Write("Set-Cookie: ", cookie.Name, "=", cookie.Value).ConfigureAwait(false);

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

            var buffer = POOL.Rent(length);

            try
            {
                var index = ASCII.GetBytes(part1, 0, part1.Length, buffer, 0);

                if (part2 is not null)
                {
                    index += ASCII.GetBytes(part2, 0, part2.Length, buffer, index);
                }

                if (part3 is not null)
                {
                    index += ASCII.GetBytes(part3, 0, part3.Length, buffer, index);
                }

                if (part4 is not null)
                {
                    index += ASCII.GetBytes(part4, 0, part4.Length, buffer, index);
                }

                if (part5 is not null)
                {
                    index += ASCII.GetBytes(part5, 0, part5.Length, buffer, index);
                }

                if (part6 is not null)
                {
                    index += ASCII.GetBytes(part6, 0, part6.Length, buffer, index);
                }

                if (part7 is not null)
                {
                    ASCII.GetBytes(part7, 0, part7.Length, buffer, index);
                }

                await OutputStream.WriteAsync(buffer.AsMemory(0, length)).ConfigureAwait(false);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
