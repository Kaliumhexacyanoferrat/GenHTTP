using System;
using System.IO;
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

        internal async Task<bool> Handle(IRequest request, IResponse response, bool keepAlive, Exception? error)
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

                await OutputStream.FlushAsync();

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

        private async Task WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == HttpProtocol.Http_1_0) ? "1.0" : "1.1";
            var status = $"{response.Status.RawStatus} {response.Status.Phrase}";

            await Write($"HTTP/{version} {status}{NL}");
        }

        private async Task WriteHeader(IResponse response, bool keepAlive)
        {
            await WriteHeaderLine("Server", $"GenHTTP/{Server.Version}");

            await WriteHeaderLine("Date", DateTime.Now);

            await WriteHeaderLine("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (!(response.ContentType is null))
            {
                await WriteHeaderLine("Content-Type", response.ContentType.RawType);
            }

            if (response.ContentEncoding != null)
            {
                await WriteHeaderLine("Content-Encoding", $"{response.ContentEncoding}");
            }

            if (response.ContentLength != null)
            {
                await WriteHeaderLine("Content-Length", $"{response.ContentLength}");
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

            foreach (var cookie in response.Cookies.Values)
            {
                await WriteCookie(cookie);
            }

            foreach (var cookie in response.RawCookies)
            {
                await WriteHeaderLine("Set-Cookie", cookie);
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

        private async Task WriteHeaderLine(string key, string value)
        {
            await Write($"{key}: {value}{NL}");
        }

        private async Task WriteHeaderLine(string key, DateTime value)
        {
            await WriteHeaderLine(key, value.ToUniversalTime().ToString("r"));
        }

        private async Task WriteCookie(Cookie cookie)
        {
            var value = $"{cookie.Name}={cookie.Value}";

            if (cookie.MaxAge != null)
            {
                value += $"; Max-Age={cookie.MaxAge.Value}";
            }

            value += "; Path=/";

            await WriteHeaderLine("Set-Cookie", value);
        }

        private async Task Write(string text)
        {
            var buffer = HEADER_ENCODING.GetBytes(text);
            await OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        #endregion

    }

}
