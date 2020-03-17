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
                WriteStatus(request, response);

                WriteHeader(response, keepAlive);

                Write(NL);

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

        private void WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";

            Write("HTTP/");
            Write(version);

            Write(" ");

            Write(response.Status.RawStatus.ToString());
            
            Write(" ");

            Write(response.Status.Phrase);

            Write(NL);
        }

        private void WriteHeader(IResponse response, bool keepAlive)
        {
            Write("Server: GenHTTP/");
            Write(Server.Version);
            Write(NL);

            WriteHeaderLine("Date", DateHeader.Value);

            WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (!(response.ContentType is null))
            {
                WriteHeaderLine("Content-Type", response.ContentType.Value.RawType);
            }

            if (response.ContentEncoding != null)
            {
                WriteHeaderLine("Content-Encoding", response.ContentEncoding!);
            }

            if (response.ContentLength != null)
            {
                WriteHeaderLine("Content-Length", response.ContentLength.ToString());
            }

            if (response.Modified != null)
            {
                WriteHeaderLine("Last-Modified", (DateTime)response.Modified);
            }

            if (response.Expires != null)
            {
                WriteHeaderLine("Expires", (DateTime)response.Expires);
            }

            if ((response.Content != null) && (response.ContentLength == null))
            {
                WriteHeaderLine("Transfer-Encoding", "chunked");
            }

            foreach (var header in response.Headers)
            {
                WriteHeaderLine(header.Key, header.Value);
            }

            foreach (var cookie in response.Cookies)
            {
                WriteCookie(cookie.Value);
            }
        }

        private async Task WriteBody(IResponse response)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteHeaderLine(string key, string value)
        {
            Write(key);
            Write(": ");
            Write(value);
            Write(NL);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteHeaderLine(string key, DateTime value)
        {
            WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteCookie(Cookie cookie)
        {
            Write("Set-Cookie: ");

            Write(cookie.Name);
            Write("=");
            Write(cookie.Value);

            if (cookie.MaxAge != null)
            {
                Write("; Max-Age=");
                Write(cookie.MaxAge.Value.ToString());
            }

            Write("; Path=/");

            Write(NL);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(string text)
        {
            var count = HEADER_ENCODING.GetByteCount(text);

            var buffer = POOL.Rent(count);

            try
            {
                HEADER_ENCODING.GetBytes(text, 0, text.Length, buffer, 0);

                OutputStream.Write(buffer, 0, count);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
