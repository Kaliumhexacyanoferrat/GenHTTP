using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Protocol
{

    internal class ResponseHandler
    {
        private static readonly string NL = "\r\n";

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

                await WriteHeader(response, keepAlive).ConfigureAwait(false);

                await Write(NL).ConfigureAwait(false);

                if (ShouldSendBody(request, response))
                {
                    await WriteBody(response).ConfigureAwait(false);
                }

                Socket.NoDelay = true;

                await OutputStream.FlushAsync().ConfigureAwait(false);

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

        private bool ShouldSendBody(IRequest request, IResponse response)
        {
            var knownStatus = response.Status.KnownStatus;

            return (request.Method.KnownMethod != RequestMethod.HEAD)
                && (knownStatus != ResponseStatus.NoContent)
                && (knownStatus != ResponseStatus.NotModified)
                && (knownStatus != ResponseStatus.PermanentRedirect)
                && (knownStatus != ResponseStatus.TemporaryRedirect)
                && (knownStatus != ResponseStatus.MovedPermanently);
        }

        private async ValueTask WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";

            await Write("HTTP/").ConfigureAwait(false);
            await Write(version).ConfigureAwait(false);

            await Write(" ").ConfigureAwait(false);

            await Write(response.Status.RawStatus.ToString()).ConfigureAwait(false);

            await Write(" ").ConfigureAwait(false);

            await Write(response.Status.Phrase).ConfigureAwait(false);

            await Write(NL).ConfigureAwait(false);
        }

        private async ValueTask WriteHeader(IResponse response, bool keepAlive)
        {
            await Write("Server: GenHTTP/").ConfigureAwait(false);
            await Write(Server.Version).ConfigureAwait(false);
            await Write(NL).ConfigureAwait(false);

            await WriteHeaderLine("Date", DateTime.UtcNow.ToString("r")).ConfigureAwait(false);

            await WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close").ConfigureAwait(false);

            if (!(response.ContentType is null))
            {
                await WriteHeaderLine("Content-Type", response.ContentType.Value.RawType).ConfigureAwait(false);
            }

            if (response.ContentEncoding != null)
            {
                await WriteHeaderLine("Content-Encoding", response.ContentEncoding!).ConfigureAwait(false);
            }

            if (response.ContentLength != null)
            {
                await WriteHeaderLine("Content-Length", response.ContentLength.ToString()).ConfigureAwait(false);
            }
            else
            {
                if (response.Content != null)
                {
                    await WriteHeaderLine("Transfer-Encoding", "chunked").ConfigureAwait(false);
                }
                else
                {
                    await WriteHeaderLine("Content-Length", "0").ConfigureAwait(false);
                }
            }

            if (response.Modified != null)
            {
                await WriteHeaderLine("Last-Modified", (DateTime)response.Modified).ConfigureAwait(false);
            }

            if (response.Expires != null)
            {
                await WriteHeaderLine("Expires", (DateTime)response.Expires).ConfigureAwait(false);
            }

            foreach (var header in response.Headers)
            {
                await WriteHeaderLine(header.Key, header.Value).ConfigureAwait(false);
            }

            foreach (var cookie in response.Cookies)
            {
                await WriteCookie(cookie.Value).ConfigureAwait(false);
            }
        }

        private async ValueTask WriteBody(IResponse response)
        {
            if (response.Content != null)
            {
                if (response.ContentLength == null)
                {
                    using var chunked = new ChunkedStream(OutputStream);

                    await response.Content.Write(chunked, Configuration.TransferBufferSize).ConfigureAwait(false);
                    
                    chunked.Finish();
                }
                else
                {
                    await response.Content.Write(OutputStream, Configuration.TransferBufferSize).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private async ValueTask WriteHeaderLine(string key, string value)
        {
            await Write(key).ConfigureAwait(false);
            await Write(": ").ConfigureAwait(false);
            await Write(value).ConfigureAwait(false);
            await Write(NL).ConfigureAwait(false);
        }

        private ValueTask WriteHeaderLine(string key, DateTime value)
        {
            return WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));
        }

        private async ValueTask WriteCookie(Cookie cookie)
        {
            await Write("Set-Cookie: ").ConfigureAwait(false);

            await Write(cookie.Name).ConfigureAwait(false);
            await Write("=").ConfigureAwait(false);
            await Write(cookie.Value).ConfigureAwait(false);

            if (cookie.MaxAge != null)
            {
                await Write("; Max-Age=").ConfigureAwait(false);
                await Write(cookie.MaxAge.Value.ToString()).ConfigureAwait(false);
            }

            await Write("; Path=/").ConfigureAwait(false);

            await Write(NL).ConfigureAwait(false);
        }

        private async ValueTask Write(string text)
        {
            var buffer = POOL.Rent(text.Length);

            try
            {
                Encoding.ASCII.GetBytes(text, 0, text.Length, buffer, 0);

                await OutputStream.WriteAsync(buffer.AsMemory(0, text.Length)).ConfigureAwait(false);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
