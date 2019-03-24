using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Patterns;

namespace GenHTTP.Core
{

    /// <summary>
    /// Provides methods to access a recieved http request.
    /// </summary>
    [Serializable]
    public class HttpRequest : IHttpRequest
    {
        private RequestType _Type;
        private ProtocolType _ProtocolType = ProtocolType.Http_1_0;
        private string _Path;
        private Dictionary<string, string> _Additional;
        private CookieCollection _Cookies;
        private ClientHandler _Handler;
        private Dictionary<string, string> _PostFields;
        private Dictionary<string, string> _GetFields;

        internal HttpRequest(ClientHandler handler)
        {
            _Additional = new Dictionary<string, string>();
            _Cookies = new CookieCollection();
            _Handler = handler;
            _PostFields = new Dictionary<string, string>();
            _GetFields = new Dictionary<string, string>();
        }

        #region get-/setters

        public IRoutingContext Routing { get; set; }

        /// <summary>
        /// The protocol supported by the client.
        /// </summary>
        public ProtocolType ProtocolType
        {
            get { return _ProtocolType; }
        }
        
        /// <summary>
        /// The type of the http request.
        /// </summary>
        public RequestType Type
        {
            get
            {
                return _Type;
            }
        }

        /// <summary>
        /// The requested file.
        /// </summary>
        public string Path
        {
            get
            {
                return _Path;
            }
        }

        /// <summary>
        /// Retrieve a header field of the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field</param>
        /// <returns>The value of the requested header field</returns>
        public string? this[string additionalHeader]
        {
            get
            {
                if (_Additional.ContainsKey(additionalHeader.ToLower())) return _Additional[additionalHeader.ToLower()];
                return null;
            }
        }

        /// <summary>
        /// The user agent of the client.
        /// </summary>
        public string? UserAgent
        {
            get
            {
                if (_Additional.ContainsKey("user-agent")) return _Additional["user-agent"];
                return null;
            }
        }

        /// <summary>
        /// All available cookies.
        /// </summary>
        public CookieCollection Cookies
        {
            get
            {
                return _Cookies;
            }
        }

        /// <summary>
        /// Data submitted via POST.
        /// </summary>
        public Dictionary<string, string> PostFields
        {
            get
            {
                return _PostFields;
            }
        }

        /// <summary>
        /// Data submitted via GET.
        /// </summary>
        public Dictionary<string, string> GetFields
        {
            get
            {
                return _GetFields;
            }
        }

        /// <summary>
        /// Retrieve a parameter from the request (GET or POST).
        /// </summary>
        /// <param name="name">The name of the parameter to retrive</param>
        /// <returns>The value of the requested parameter or null, if it could not be found</returns>
        /// <remarks>
        /// This method will prioritize POST parameters over GET parameters.
        /// </remarks>
        public string? GetParameter(string name)
        {
            if (PostFields.ContainsKey(name)) return PostFields[name];
            if (GetFields.ContainsKey(name)) return GetFields[name];
            return null;
        }

        /// <summary>
        /// The address (URI) of the resource from which the Request-URI was obtained.
        /// </summary>
        public string? Referer
        {
            get
            {
                if (_Additional.ContainsKey("referer")) return _Additional["referer"];
                return null;
            }
        }

        /// <summary>
        /// The client handler assigned to this request.
        /// </summary>
        public IClientHandler Handler
        {
            get
            {
                return _Handler;
            }
        }

        /// <summary>
        /// The requested host.
        /// </summary>
        /// <remarks>
        /// Used for virtual hosting.
        /// </remarks>
        public string? Host
        {
            get
            {
                if (_Additional.ContainsKey("host")) return _Additional["host"];
                return null;
            }
        }

        #endregion

        #region parser-support

        // ToDo: Rework this section

        internal void ParseType(string type)
        {
            if (type == "POST") { _Type = RequestType.POST; return; }
            if (type == "HEAD") { _Type = RequestType.HEAD; return; }
            _Type = RequestType.GET;
        }

        internal void ParseHttp(string version)
        {
            if (version == "1.0") _ProtocolType = ProtocolType.Http_1_0;
            _ProtocolType = ProtocolType.Http_1_1;
        }

        internal void ParseURL(string URL)
        {
            _Path = URL;

            // read GET parameters
            int pos = _Path.IndexOf('?');

            if (pos > -1)
            {
                string getPart = (_Path.Length > pos) ? _Path.Substring(pos + 1) : "";

                PatternGetParameter get = new PatternGetParameter();

                foreach (Match m in get.Matches(getPart))
                {
                    // add this get parameter only, if it does not exist yet
                    if (!_GetFields.ContainsKey(m.Groups[1].Value))
                        _GetFields.Add(m.Groups[1].Value, Uri.UnescapeDataString(m.Groups[2].Value.Replace('+', ' ')));
                }

                _Path = _Path.Substring(0, pos);
            }
        }

        internal void ParseHeaderField(string field, string value)
        {
            if (field.ToLower() == "cookie")
            {
                string[] cookies = value.Split("; ".ToCharArray());
                foreach (string cookie in cookies)
                {
                    int pos = cookie.IndexOf("=");
                    if (pos > -1)
                    {
                        string name = cookie.Substring(0, pos);
                        string val = cookie.Substring(pos + 1);
                        if (!_Cookies.Exists(name)) _Cookies.Add(name, val);
                    }
                }
            }
            else
            {
                if (!_Additional.ContainsKey(field.ToLower())) _Additional.Add(field.ToLower(), value);
            }
        }

        internal void ParseBody(string body)
        {
            if (_Type == RequestType.POST)
            {
                string[] fields = body.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string field in fields)
                {
                    int pos = field.IndexOf("=");
                    if (pos > -1)
                    {
                        string name = field.Substring(0, pos);
                        string value = field.Substring(pos + 1);
                        // add, if there is no field with this name
                        if (!_PostFields.ContainsKey(name)) _PostFields.Add(name, DecodeData(value));
                    }
                }
            }
        }

        /// <summary>
        /// Decode hexadecimal encoded data (e.g. %20).
        /// </summary>
        /// <param name="toDecode">The string to decode</param>
        /// <returns>The decoded string</returns>
        private string DecodeData(string toDecode)
        {
            toDecode = toDecode.Replace("+", " ");
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < toDecode.Length; i++)
            {
                // current entity does not start with a %
                if (!(toDecode[i] == '%')) { ret.Append(toDecode[i]); continue; }
                // maybe got a hexadecimal entity => read the length
                if (IsHex(toDecode[i + 1]) && IsHex(toDecode[i + 2]))
                {
                    ret.Append(Char.ConvertFromUtf32(Convert.ToInt32(toDecode.Substring(i + 1, 2), 16)));
                    // don't forget that we did some look forward
                    i += 2;
                }
                else
                {
                    ret.Append("%");
                }
            }
            return ret.ToString();
        }

        private bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'F') ||
                   (c >= 'A' && c <= 'F');
        }

        #endregion
                        
    }

}
