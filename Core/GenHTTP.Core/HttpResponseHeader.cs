using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction;

namespace GenHTTP
{

    /// <summary>
    /// Available response types.
    /// </summary>
    [Serializable]
    public enum ResponseType
    {
        /// <summary>
        /// 200
        /// </summary>
        OK,
        /// <summary>
        /// 201
        /// </summary>
        Created,
        /// <summary>
        /// 202
        /// </summary>
        Accepted,
        /// <summary>
        /// 204
        /// </summary>
        NoContent,
        /// <summary>
        /// 301
        /// </summary>
        MovedPermanently,
        /// <summary>
        /// 302
        /// </summary>
        MovedTemporarily,
        /// <summary>
        /// 304
        /// </summary>
        NotModified,
        /// <summary>
        /// 400
        /// </summary>
        BadRequest,
        /// <summary>
        /// 401
        /// </summary>
        Unauthorized,
        /// <summary>
        /// 403
        /// </summary>
        Forbidden,
        /// <summary>
        /// 404
        /// </summary>
        NotFound,
        /// <summary>
        /// 500
        /// </summary>
        InternalServerError,
        /// <summary>
        /// 501
        /// </summary>
        NotImplemented,
        /// <summary>
        /// 502
        /// </summary>
        BadGateway,
        /// <summary>
        /// 503
        /// </summary>
        ServiceUnavailable
    }

    /// <summary>
    /// The content type of the response.
    /// </summary>
    [Serializable]
    public enum ContentType
    {
        /// <summary>
        /// A html page.
        /// </summary>
        TextHtml,
        /// <summary>
        /// A stylesheet.
        /// </summary>
        TextCss,
        /// <summary>
        /// Only because IE8 is really buggy.
        /// </summary>
        TextJavaScript,
        /// <summary>
        /// A JavaScript source file.
        /// </summary>
        ApplicationJavaScript,
        /// <summary>
        /// A PNG image.
        /// </summary>
        ImagePng,
        /// <summary>
        /// A BMP image.
        /// </summary>
        ImageBmp,
        /// <summary>
        /// A JPG image.
        /// </summary>
        ImageJpg,
        /// <summary>
        /// A GIF image.
        /// </summary>
        ImageGif,
        /// <summary>
        /// A download.
        /// </summary>
        ApplicationForceDownload,
        /// <summary>
        /// A MP4 audio file.
        /// </summary>
        AudioMp4,
        /// <summary>
        /// A OGG audio file.
        /// </summary>
        AudioOgg,
        /// <summary>
        /// A MPEG audio file.
        /// </summary>
        AudioMpeg,
        /// <summary>
        /// A TIFF image.
        /// </summary>
        ImageTiff,
        /// <summary>
        /// A CSV file.
        /// </summary>
        TextCsv,
        /// <summary>
        /// A RTF file.
        /// </summary>
        TextRichText,
        /// <summary>
        /// Plain text.
        /// </summary>
        TextPlain,
        /// <summary>
        /// A XML file.
        /// </summary>
        TextXml,
        /// <summary>
        /// A H.264 encoded video file.
        /// </summary>
        VideoH264,
        /// <summary>
        /// A MP4 video file.
        /// </summary>
        VideoMp4,
        /// <summary>
        /// A MPEG video file.
        /// </summary>
        VideoMpeg,
        /// <summary>
        /// A MPEG-4 video file.
        /// </summary>
        VideoMpeg4Generic,
        /// <summary>
        /// A uncompressed audio file.
        /// </summary>
        AudioWav,
        /// <summary>
        /// Word processing document (e.g. docx).
        /// </summary>
        ApplicationOfficeDocumentWordProcessing,
        /// <summary>
        /// A presentation (e.g. pptx).
        /// </summary>
        ApplicationOfficeDocumentPresentation,
        /// <summary>
        /// A slideshow (e.g. .ppsx).
        /// </summary>
        ApplicationOfficeDocumentSlideshow,
        /// <summary>
        /// A sheet (e.g. .xlsx).
        /// </summary>
        ApplicationOfficeDocumentSheet,
        /// <summary>
        /// An icon.
        /// </summary>
        ImageIcon
    }

    /// <summary>
    /// The type of protocol to use for the response.
    /// </summary>
    [Serializable]
    public enum ProtocolType
    {
        /// <summary>
        /// HTTP/1.0
        /// </summary>
        Http_1_0,
        /// <summary>
        /// HTTP/1.1
        /// </summary>
        Http_1_1
    }

    /// <summary>
    /// The HTTP header of a HTTP response.
    /// </summary>
    [Serializable]
    public class HttpResponseHeader : MarshalByRefObject
    {
        private ResponseType _Type;
        private ContentType _ContentType;
        private Dictionary<string, string> _Fields;
        private List<HttpCookie> _Cookies;
        private HttpResponse _Response;
        private DateTime _Expires;
        private bool _DoesExpire = false;
        private DateTime _Modified;
        private bool _IsModified = false;
        private bool _Close = false;
        private bool _KeepAlive;
        private ProtocolType _ProtocolType;
        private Encoding _ContentEncoding = Encoding.UTF8;
        private LanguageInfo _ContentLanguage;
        private bool _SendEncodingInfo = false;

        /// <summary>
        /// Create a new HTTP response header.
        /// </summary>
        /// <param name="response">The response this header relates to</param>
        /// <param name="protocolType">The protocol, the response should use</param>
        /// <param name="keepAlive">Specify, whether the connection should keep alive</param>
        internal HttpResponseHeader(HttpResponse response, ProtocolType protocolType, bool keepAlive)
        {
            _KeepAlive = keepAlive;
            _ProtocolType = protocolType;
            _Response = response;
            _Fields = new Dictionary<string, string>();
            _Fields.Add("Server", "GenHTTP/" + response.ClientHandler.Server.Version);
            if (_Close) _Fields.Add("Connection", "close");
            else if (keepAlive) _Fields.Add("Connection", "Keep-Alive");
            _Fields.Add("Date", DateTime.Now.ToUniversalTime().ToString("r"));
            _Type = ResponseType.OK;
            _ContentType = ContentType.TextHtml;
            _Cookies = new List<HttpCookie>();
        }

        #region get-/setters

        /// <summary>
        /// Specify, whether the connection should get closed after this request or not.
        /// </summary>
        public bool CloseConnection
        {
            get { return _Close; }
            set { _Close = value; }
        }

        /// <summary>
        /// Retrieve or set the value of a header field.
        /// </summary>
        /// <param name="field">The name of the header field</param>
        /// <returns>The value of the header field</returns>
        public string this[string field]
        {
            get
            {
                if (_Fields.ContainsKey(field)) return _Fields[field];
                return "";
            }
            set
            {
                if (_Fields.ContainsKey(field))
                {
                    _Fields[field] = value;
                    return;
                }
                _Fields.Add(field, value);
            }
        }

        /// <summary>
        /// The HTTP respnse code.
        /// </summary>
        public ResponseType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        /// <summary>
        /// The type of the content.
        /// </summary>
        public ContentType ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                _ContentType = value;
            }
        }

        /// <summary>
        /// The charset used to encode the data.
        /// </summary>
        /// <remarks>
        /// If you set this value manually, the <see cref="SendEncodingInfo"/>
        /// property will be set to true.
        /// </remarks>
        public Encoding ContentEncoding
        {
            get { return _ContentEncoding; }
            set { _ContentEncoding = value; _SendEncodingInfo = true; }
        }

        /// <summary>
        /// Specify, whether the "charset" information
        /// should be sent.
        /// </summary>
        public bool SendEncodingInfo
        {
            get { return _SendEncodingInfo; }
            set { _SendEncodingInfo = value; }
        }

        /// <summary>
        /// The version of the HTTP protocol.
        /// </summary>
        public ProtocolType ProtocolType
        {
            get { return _ProtocolType; }
        }

        /// <summary>
        /// Define, when this ressource will expire.
        /// </summary>
        public DateTime Expires
        {
            get { return _Expires; }
            set { _Expires = value; _DoesExpire = true; }
        }

        /// <summary>
        /// Specifies, whether the expires header field is set.
        /// </summary>
        public bool DoesExpire
        {
            get { return _DoesExpire; }
        }

        /// <summary>
        /// Define, when this ressource has been changed the last time.
        /// </summary>
        public DateTime Modified
        {
            get { return _Modified; }
            set { _Modified = value; _IsModified = true; }
        }

        /// <summary>
        /// Specifies, whether the modified header field is set.
        /// </summary>
        public bool IsModified
        {
            get { return _IsModified; }
        }

        /// <summary>
        /// Retrieve the status code of this response (Type).
        /// </summary>
        public int Status
        {
            get
            {
                if (Type == ResponseType.OK) return 200;
                if (Type == ResponseType.NotFound) return 404;
                if (Type == ResponseType.Accepted) return 202;
                if (Type == ResponseType.BadGateway) return 502;
                if (Type == ResponseType.BadRequest) return 400;
                if (Type == ResponseType.Created) return 201;
                if (Type == ResponseType.Forbidden) return 403;
                if (Type == ResponseType.InternalServerError) return 500;
                if (Type == ResponseType.MovedPermanently) return 301;
                if (Type == ResponseType.MovedTemporarily) return 302;
                if (Type == ResponseType.NoContent) return 204;
                if (Type == ResponseType.NotImplemented) return 501;
                if (Type == ResponseType.NotModified) return 304;
                if (Type == ResponseType.ServiceUnavailable) return 503;
                if (Type == ResponseType.Unauthorized) return 401;
                return 400;
            }
        }

        /// <summary>
        /// Information about the language of the content to send.
        /// </summary>
        public GenHTTP.Abstraction.LanguageInfo ContentLanguage
        {
            get { return _ContentLanguage; }
            set { _ContentLanguage = value; }
        }

        #endregion

        /// <summary>
        /// Add a cookie to send to the client.
        /// </summary>
        /// <param name="cookie">The cookie to send</param>
        public void AddCookie(HttpCookie cookie)
        {
            _Cookies.Add(cookie);
        }

        /// <summary>
        /// Write the header information to the connected socket.
        /// </summary>
        /// <param name="handler">The handler to write to</param>
        /// <param name="contentLength">The length of the content the response contains</param>
        internal void WriteHeader(ClientHandler handler, ulong contentLength)
        {
            string nl = Environment.NewLine;
            // PROTCOL VERSION AND STATUS CODE
            if (_ProtocolType == ProtocolType.Http_1_0)
            {
                handler.SendBytes(StringToBytes("HTTP/1.0 " + GetStatusCode(_Type) + nl));
            }
            else
            {
                handler.SendBytes(StringToBytes("HTTP/1.1 " + GetStatusCode(_Type) + nl));
            }
            // CONTENT-TYPE
            string contentType = GetContentType(_ContentType);
            handler.SendBytes(StringToBytes("Content-Type: " + contentType));
            if (_SendEncodingInfo) handler.SendBytes(StringToBytes("; charset=" + _ContentEncoding.WebName));
            handler.SendBytes(StringToBytes(nl));
            // CONTENT-LENGTH
            if (contentLength > 0)
            {
                handler.SendBytes(StringToBytes("Content-Length: " + contentLength + nl));
            }
            // CONTENT-ENCODING
            // note: if the whole content is compressable cause of its length, the compressed content MUST be, too.
            if (_Response.IsContentCompressable((long)contentLength) && _Response.UseCompression)
            {
                handler.SendBytes(StringToBytes("Content-Encoding: gzip" + nl));
            }
            // CONTENT-LANGUAGE
            if (_ContentLanguage != null)
            {
                handler.SendBytes(StringToBytes("Content-Language: " + _ContentLanguage.LanguageString + nl));
            }
            // LAST-MODIFIED
            if (_IsModified)
            {
                handler.SendBytes(StringToBytes("Last-Modified: " + _Modified.ToUniversalTime().ToString("r") + nl));
            }
            // EXPIRES
            if (_DoesExpire)
            {
                handler.SendBytes(StringToBytes("Expires: " + _Expires.ToUniversalTime().ToString("r") + nl));
            }
            // ADDITIONAL HEADER FIELDS
            foreach (string header in _Fields.Keys)
            {
                handler.SendBytes(StringToBytes(header + ": " + this[header] + nl));
            }
            // SET-COOKIE
            foreach (HttpCookie cookie in _Cookies)
            {
                handler.SendBytes(StringToBytes(cookie.ToHttp() + nl));
            }
            handler.SendBytes(StringToBytes(nl));
        }

        private byte[] StringToBytes(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        /// <summary>
        /// Retrieve the content type string of a content type.
        /// </summary>
        /// <param name="type">The type to convert</param>
        /// <returns>The converted string</returns>
        public static string GetContentType(ContentType type)
        {
            if (type == ContentType.TextHtml) return "text/html";
            if (type == ContentType.TextCss) return "text/css";
            if (type == ContentType.ApplicationJavaScript) return "application/javascript";
            if (type == ContentType.TextJavaScript) return "text/javascript";
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

        /// <summary>
        /// Try to retrieve the <see cref="ContentType" /> of a file by its extension.
        /// </summary>
        /// <param name="extension">The extension of the file (without the dot)</param>
        /// <returns>The content type to send the file with</returns>
        public static ContentType GetContentTypeByExtension(string extension)
        {
            if (extension == null) return ContentType.ApplicationForceDownload;
            switch (extension.ToLower())
            {
                // CSS file
                case "css": return ContentType.TextCss;
                // HTML files
                case "html":
                case "htm": return ContentType.TextHtml;
                case "ico": return ContentType.ImageIcon;
                // Text files
                case "sql":
                case "txt":
                case "pl":
                case "cs":
                case "php":
                case "c":
                case "h":
                case "cpp":
                case "sh":
                case "bat":
                case "cmd":
                case "conf":
                case "ini":
                case "inf": return ContentType.TextPlain;
                // JavaScript
                case "js": return ContentType.ApplicationJavaScript;
                // GIF
                case "gif": return ContentType.ImageGif;
                // JPG
                case "jpeg":
                case "jpg": return ContentType.ImageJpg;
                // PNG
                case "png": return ContentType.ImagePng;
                // BMP
                case "bmp": return ContentType.ImageBmp;
                // OGG
                case "ogg": return ContentType.AudioOgg;
                // MP3
                case "mp3": return ContentType.AudioMpeg;
                // WAV
                case "wav": return ContentType.AudioWav;
                // TIFF
                case "tiff": return ContentType.ImageTiff;
                // CSV
                case "csv": return ContentType.TextCsv;
                // RTF
                case "rtf": return ContentType.TextRichText;
                // XML
                case "xml": return ContentType.TextXml;
                // Video files
                case "mpg":
                case "mpeg":
                case "avi": return ContentType.VideoMpeg;
                // Word processing
                case "docx": return ContentType.ApplicationOfficeDocumentWordProcessing;
                // Presentation
                case "pptx": return ContentType.ApplicationOfficeDocumentPresentation;
                // Slideshow
                case "ppsx": return ContentType.ApplicationOfficeDocumentSlideshow;
                // Sheet
                case "xlsx": return ContentType.ApplicationOfficeDocumentSheet;
                // All other files
                default: return ContentType.ApplicationForceDownload;
            }
        }

        /// <summary>
        /// Retrieve the name of a status by its code.
        /// </summary>
        /// <param name="type">The response type to convert</param>
        /// <returns>The name of the status</returns>
        public string GetStatusCode(ResponseType type)
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
            return "400 Bad Request";
        }

    }

}
