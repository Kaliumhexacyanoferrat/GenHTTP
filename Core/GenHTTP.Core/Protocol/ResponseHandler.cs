using System;
using System.Buffers;
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
        private static ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

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

                if (request.Method != RequestMethod.HEAD)
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

        private async Task WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == ProtocolType.Http_1_0) ? "1.0" : "1.1";
            var status = $"{response.Status.RawStatus} {response.Status.Phrase}";

            await Write($"HTTP/{version} {status}{NL}");
        }

        private async Task WriteHeader(IResponse response, bool keepAlive)
        {
            await WriteHeader("Server", $"GenHTTP/{Server.Version}");

            await WriteHeader("Date", DateTime.Now);

            await WriteHeader("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (!(response.ContentType is null))
            {
                await WriteHeader("Content-Type", response.ContentType.RawType);
            }

            if (response.ContentEncoding != null)
            {
                await WriteHeader("Content-Encoding", $"{response.ContentEncoding}");
            }

            if (response.ContentLength != null)
            {
                await WriteHeader("Content-Length", $"{response.ContentLength}");
            }

            if (response.Modified != null)
            {
                await WriteHeader("Modified", (DateTime)response.Modified);
            }

            if (response.Expires != null)
            {
                await WriteHeader("Expires", (DateTime)response.Expires);
            }

            if ((response.Content != null) && (response.ContentLength == null))
            {
                await WriteHeader("Transfer-Encoding", "chunked");
            }

            foreach (var header in response.Headers)
            {
                await WriteHeader(header.Key, header.Value);
            }

            foreach (var cookie in response.Cookies.Values)
            {
                await WriteCookie(cookie);
            }
        }

        private async Task WriteBody(IResponse response)
        {
            if (response.Content != null)
            {
                if (response.Content.CanSeek && (response.Content.Position != 0))
                {
                    response.Content.Seek(0, SeekOrigin.Begin);
                }
                
                if ((response.Content != null) && (response.ContentLength == null))
                {
                    int read;

                    var buffer = POOL.Rent((int)Configuration.TransferBufferSize);

                    try
                    {
                        do
                        {
                            read = await response.Content.ReadAsync(buffer, 0, buffer.Length);

                            if (read > 0)
                            {
                                await Write($"{read.ToString("X")}{NL}");
                                await OutputStream.WriteAsync(buffer, 0, read);
                                await Write(NL);
                            }
                        }
                        while (read > 0);
                    }
                    finally
                    {
                        POOL.Return(buffer);
                    }

                    await Write($"0{NL}{NL}");
                }
                else
                {
                    response.Content.CopyTo(OutputStream, (int)Configuration.TransferBufferSize);
                }                    
            }
        }

        #endregion

        #region Helpers

        private async Task WriteHeader(string key, string value)
        {
            await Write($"{key}: {value}{NL}");
        }

        private async Task WriteHeader(string key, DateTime value)
        {
            await WriteHeader(key, value.ToUniversalTime().ToString("r"));
        }

        private async Task WriteCookie(Cookie cookie)
        {
            var value = $"{cookie.Name}={cookie.Value}";

            if (cookie.Expires != null)
            {
                var t = (DateTime)cookie.Expires;
                value += $"; expires={t.ToUniversalTime().ToString("r")}";
            }

            if (cookie.MaxAge != null)
            {
                value += $"; Max-Age={cookie.MaxAge.Value}";
            }

            value += "; Path=/";

            await WriteHeader("Set-Cookie", value);
        }

        private async Task Write(string text)
        {
            var buffer = HEADER_ENCODING.GetBytes(text);
            await OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
                
        #endregion

    }

}
