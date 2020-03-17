using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Protocol
{

    internal class ResponseHandler
    {
        private static readonly Encoding HEADER_ENCODING = Encoding.GetEncoding("ISO-8859-1");

        private static readonly string NL = "\r\n";

        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;
        
        #region Get-/Setters

        private IServer Server { get; }

        private Stream OutputStream { get; }

        internal NetworkConfiguration Configuration { get; }

        #endregion

        #region Initialization

        internal ResponseHandler(IServer server, Stream outputstream, NetworkConfiguration configuration)
        {
            Server = server;
            OutputStream = outputstream;
            Configuration = configuration;
        }

        #endregion

        #region Functionality

        internal async ValueTask<bool> Handle(IRequest request, IResponse response, bool keepAlive, Exception? error)
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

                Server.Companion?.OnRequestHandled(request, response, error);

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
                && (knownStatus != ResponseStatus.TemporaryRedirect);
        }

        private async ValueTask WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";

            await Write("HTTP/");
            await Write(version);

            await Write(" ");

            await Write(response.Status.RawStatus.ToString());

            await Write(" ");

            await Write(response.Status.Phrase);

            await Write(NL);
        }

        private async ValueTask WriteHeader(IResponse response, bool keepAlive)
        {
            await Write("Server: GenHTTP/");
            await Write(Server.Version);
            await Write(NL);

            await WriteHeaderLine("Date", DateHeader.Value);

            await WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (!(response.ContentType is null))
            {
                await WriteHeaderLine("Content-Type", response.ContentType.Value.RawType);
            }

            if (response.ContentEncoding != null)
            {
                await WriteHeaderLine("Content-Encoding", response.ContentEncoding!);
            }

            if (response.ContentLength != null)
            {
                await WriteHeaderLine("Content-Length", response.ContentLength.ToString());
            }

            if (response.Modified != null)
            {
                await WriteHeaderLine("Last-Modified", (DateTime)response.Modified);
            }

            if (response.Expires != null)
            {
                await WriteHeaderLine("Expires", (DateTime)response.Expires);
            }

            if ((response.Content != null) && (response.ContentLength == null))
            {
                await WriteHeaderLine("Transfer-Encoding", "chunked");
            }

            foreach (var header in response.Headers)
            {
                await WriteHeaderLine(header.Key, header.Value);
            }

            foreach (var cookie in response.Cookies)
            {
                await WriteCookie(cookie.Value);
            }
        }

        private async ValueTask WriteBody(IResponse response)
        {
            if (response.Content != null)
            {
                if (response.ContentLength == null)
                {
                    using var chunked = new ChunkedStream(OutputStream);

                    await response.Content.Write(chunked, Configuration.TransferBufferSize);
                    
                    chunked.Finish();
                }
                else
                {
                    await response.Content.Write(OutputStream, Configuration.TransferBufferSize);
                }
            }
        }

        #endregion

        #region Helpers

        private async ValueTask WriteHeaderLine(string key, string value)
        {
            await Write(key);
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
            await Write("Set-Cookie: ");

            await Write(cookie.Name);
            await Write("=");
            await Write(cookie.Value);

            if (cookie.MaxAge != null)
            {
                await Write("; Max-Age=");
                await Write(cookie.MaxAge.Value.ToString());
            }

            await Write("; Path=/");

            await Write(NL);
        }

        private async ValueTask Write(string text)
        {
            var count = HEADER_ENCODING.GetByteCount(text);

            var buffer = POOL.Rent(count);

            try
            {
                HEADER_ENCODING.GetBytes(text, 0, text.Length, buffer, 0);

                await OutputStream.WriteAsync(buffer.AsMemory(0, count));
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
