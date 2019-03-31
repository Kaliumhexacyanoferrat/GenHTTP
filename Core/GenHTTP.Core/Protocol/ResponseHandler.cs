using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class ResponseHandler
    {
        private static readonly Encoding HEADER_ENCODING = Encoding.GetEncoding("ISO-8859-1");

        private static readonly string NL = "\r\n";

        #region Get-/Setters

        protected IServer Server { get; }

        protected Stream OutputStream { get; }

        #endregion

        #region Initialization

        internal ResponseHandler(IServer server, Stream outputstream)
        {
            Server = server;
            OutputStream = outputstream;
        }

        #endregion

        #region Functionality

        internal bool Handle(IRequest request, IResponse response, bool keepAlive, Exception? error)
        {
            try
            {
                WriteStatus(request, response);

                WriteHeader(response, keepAlive);

                Write(NL);

                if (request.Type != RequestType.HEAD)
                {
                    WriteBody(response);
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

        private void WriteStatus(IRequest request, IResponse response)
        {
            var version = (request.ProtocolType == ProtocolType.Http_1_0) ? "1.0" : "1.1";
            var status = GetStatusPhrase(response.Type);

            Write($"HTTP/{version} {status}{NL}");
        }

        private void WriteHeader(IResponse response, bool keepAlive)
        {
            WriteHeader("Server", $"GenHTTP/{Server.Version}");

            WriteHeader("Date", DateTime.Now);

            WriteHeader("Connection", (keepAlive) ? "Keep-Alive" : "Close");

            if (response.ContentType != null)
            {
                var contentType = GetContentType((ContentType)response.ContentType);

                if (response.ContentEncoding != null)
                {
                    WriteHeader("Content-Type", $"{contentType}; charset={response.ContentEncoding.WebName}");
                }
                else
                {
                    WriteHeader("Content-Type", contentType);
                }
            }

            if (response.ContentLength != null)
            {
                WriteHeader("Content-Length", $"{response.ContentLength}");
            }

            if (response.Modified != null)
            {
                WriteHeader("Modified", (DateTime)response.Modified);
            }

            if (response.Expires != null)
            {
                WriteHeader("Expires", (DateTime)response.Expires);
            }

            foreach (var header in response.Headers)
            {
                WriteHeader(header.Key, header.Value);
            }

            foreach (var cookie in response.Cookies.Values)
            {
                WriteCookie(cookie);
            }
        }

        private void WriteBody(IResponse response)
        {
            if (response.Content != null)
            {
                if (response.Content.CanSeek && (response.Content.Position != 0))
                {
                    response.Content.Seek(0, SeekOrigin.Begin);
                }

                response.Content.CopyTo(OutputStream);
            }
        }

        #endregion

        #region Helpers

        private void WriteHeader(string key, string value)
        {
            Write($"{key}: {value}{NL}");
        }

        private void WriteHeader(string key, DateTime value)
        {
            WriteHeader(key, value.ToUniversalTime().ToString("r"));
        }
        
        private void WriteCookie(Cookie cookie)
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

            WriteHeader("Set-Cookie", value);
        }
        
        private void Write(string text)
        {
            var buffer = HEADER_ENCODING.GetBytes(text);
            OutputStream.Write(buffer, 0, buffer.Length);
        }

        private static string GetStatusPhrase(ResponseType type)
        {
            if (type == ResponseType.Accepted) return "202 Accepted";
            if (type == ResponseType.BadGateway) return "502 Bad Gateway";
            if (type == ResponseType.BadRequest) return "400 Bad Request";
            if (type == ResponseType.Created) return "201 Created";
            if (type == ResponseType.Forbidden) return "403 Forbidden";
            if (type == ResponseType.InternalServerError) return "500 Internal Server Error";
            if (type == ResponseType.MovedPermanently) return "301 Moved Permanently";
            if (type == ResponseType.MovedTemporarily) return "302 Moved Temporarily";
            if (type == ResponseType.NoContent) return "204 No Content";
            if (type == ResponseType.NotFound) return "404 Not Found";
            if (type == ResponseType.NotImplemented) return "501 Not Implemented";
            if (type == ResponseType.NotModified) return "304 Not Modified";
            if (type == ResponseType.OK) return "200 OK";
            if (type == ResponseType.ServiceUnavailable) return "503 Service Unavailable";
            if (type == ResponseType.Unauthorized) return "401 Unauthorized";
            return "500 Internal Server Error";
        }

        private static string GetContentType(ContentType type)
        {
            if (type == ContentType.TextHtml) return "text/html";
            if (type == ContentType.TextCss) return "text/css";
            if (type == ContentType.ApplicationJavaScript) return "application/javascript";
            if (type == ContentType.ImageIcon) return "image/vnd.microsoft.icon";
            if (type == ContentType.ImageGif) return "image/gif";
            if (type == ContentType.ImageJpg) return "image/jpg";
            if (type == ContentType.ImagePng) return "image/png";
            if (type == ContentType.ImageBmp) return "image/bmp";
            if (type == ContentType.AudioMp4) return "audio/mp4";
            if (type == ContentType.AudioOgg) return "audio/ogg";
            if (type == ContentType.AudioMpeg) return "audio/mpeg";
            if (type == ContentType.ImageTiff) return "image/tiff";
            if (type == ContentType.TextCsv) return "text/csv";
            if (type == ContentType.TextRichText) return "text/richtext";
            if (type == ContentType.TextPlain) return "text/plain";
            if (type == ContentType.TextXml) return "text/xml";
            if (type == ContentType.VideoH264) return "video/H264";
            if (type == ContentType.VideoMp4) return "video/mp4";
            if (type == ContentType.VideoMpeg) return "video/mpeg";
            if (type == ContentType.VideoMpeg4Generic) return "video/mpeg4-generic";
            if (type == ContentType.AudioWav) return "audio/wav";
            if (type == ContentType.ApplicationOfficeDocumentWordProcessing) return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            if (type == ContentType.ApplicationOfficeDocumentPresentation) return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            if (type == ContentType.ApplicationOfficeDocumentSlideshow) return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
            if (type == ContentType.ApplicationOfficeDocumentSheet) return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return "application/force-download";
        }

        #endregion
        
    }

}
