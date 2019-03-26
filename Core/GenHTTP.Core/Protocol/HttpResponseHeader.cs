using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Core
{
    
    /// <summary>
    /// The HTTP header of a HTTP response.
    /// </summary>
    [Serializable]
    public class HttpResponseHeader : IHttpResponseHeader
    {
        private ResponseType _Type;
        private ContentType _ContentType;
        private Dictionary<string, string> _Fields;
        private List<HttpCookie> _Cookies;
        private HttpResponse _Response;
        private DateTime? _Expires;
        private DateTime? _Modified;
        private bool _Close = false;
        private bool _KeepAlive;
        private ProtocolType _ProtocolType;
        private Encoding? _ContentEncoding;

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

            _Fields = new Dictionary<string, string>
            {
                { "Server", "GenHTTP/" + response.ClientHandler.Server.Version }
            };

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
        public Encoding? ContentEncoding
        {
            get { return _ContentEncoding; }
            set { _ContentEncoding = value; }
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
        public DateTime? Expires
        {
            get { return _Expires; }
            set { _Expires = value; }
        }

        /// <summary>
        /// Define, when this ressource has been changed the last time.
        /// </summary>
        public DateTime? Modified
        {
            get { return _Modified; }
            set { _Modified = value; }
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
        internal void WriteHeader(ClientHandler handler, ulong? contentLength)
        {
            var nl = "\r\n";

            // PROTCOL VERSION AND STATUS CODE
            if (_ProtocolType == ProtocolType.Http_1_0)
            {
                handler.Send(StringToBytes("HTTP/1.0 " + Mapping.GetStatusCode(_Type) + nl));
            }
            else
            {
                handler.Send(StringToBytes("HTTP/1.1 " + Mapping.GetStatusCode(_Type) + nl));
            }

            // CONTENT-TYPE
            string contentType = Mapping.GetContentType(_ContentType);
            handler.Send(StringToBytes("Content-Type: " + contentType));

            if (_ContentEncoding != null)
            {
                handler.Send(StringToBytes("; charset=" + _ContentEncoding.WebName));
            }

            handler.Send(StringToBytes(nl));
            
            // CONTENT-LENGTH
            if (contentLength != null && contentLength > 0)
            {
                handler.Send(StringToBytes("Content-Length: " + contentLength + nl));
            }

            // LAST-MODIFIED
            if (_Modified != null)
            {
                handler.Send(StringToBytes("Last-Modified: " + ((DateTime)_Modified).ToUniversalTime().ToString("r") + nl));
            }

            // EXPIRES
            if (_Expires != null)
            {
                handler.Send(StringToBytes("Expires: " + ((DateTime)_Expires).ToUniversalTime().ToString("r") + nl));
            }

            // ADDITIONAL HEADER FIELDS
            foreach (string header in _Fields.Keys)
            {
                handler.Send(StringToBytes(header + ": " + this[header] + nl));
            }

            // SET-COOKIE
            foreach (var cookie in _Cookies)
            {
                handler.Send(StringToBytes(ToHttp(cookie) + nl));
            }

            handler.Send(StringToBytes(nl));
        }

        private byte[] StringToBytes(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        internal string ToHttp(HttpCookie cookie)
        {
            string text = "Set-Cookie: " + cookie.Name + "=" + cookie.Value;
            if (cookie.Expires.HasValue)
            {
                DateTime t = cookie.Expires.Value;
                text += "; expires=" + GetDayOfWeek(t) + ", " + GetTwoDigits(t.Day) + "-" + GetMonth(t) + "-" + t.Year + " " + GetTwoDigits(t.Hour) + ":" + GetTwoDigits(t.Minute) + ":" + GetTwoDigits(t.Second) + " GMT";
            }
            if (cookie.MaxAge.HasValue)
            {
                text += "; Max-Age=" + cookie.MaxAge.Value;
            }
            text += "; Path=/";
            return text;
        }

        private string GetDayOfWeek(DateTime t)
        {
            switch (t.DayOfWeek)
            {
                case DayOfWeek.Monday: return "Mon";
                case DayOfWeek.Tuesday: return "Tue";
                case DayOfWeek.Wednesday: return "Wed";
                case DayOfWeek.Thursday: return "Thu";
                case DayOfWeek.Friday: return "Fri";
                case DayOfWeek.Saturday: return "Sat";
                case DayOfWeek.Sunday: return "Sun";
            }
            return "Mon";
        }

        private string GetTwoDigits(int t)
        {
            if (t < 10) return "0" + t;
            return t.ToString();
        }

        private string GetMonth(DateTime t)
        {
            if (t.Month == 1) return "Jan";
            if (t.Month == 2) return "Feb";
            if (t.Month == 3) return "Mar";
            if (t.Month == 4) return "Apr";
            if (t.Month == 5) return "May";
            if (t.Month == 6) return "Jun";
            if (t.Month == 7) return "Jul";
            if (t.Month == 8) return "Aug";
            if (t.Month == 9) return "Sep";
            if (t.Month == 10) return "Okt";
            if (t.Month == 11) return "Nov";
            if (t.Month == 12) return "Dec";
            return "Jan";
        }

    }

}
