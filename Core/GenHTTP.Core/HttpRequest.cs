using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GenHTTP.Api.Http;
using GenHTTP.Api.Project;
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
        private string _File;
        private Dictionary<string, string> _Additional;
        private CookieCollection _Cookies;
        private IProject _Project;
        private ClientHandler _Handler;
        private Dictionary<string, string> _PostFields;
        private Dictionary<string, string> _GetFields;
        private bool _Redirected = false;
        private List<string> _RedirectedFrom;
        private bool _VirtualHosting;

        internal HttpRequest(ClientHandler handler)
        {
            _Additional = new Dictionary<string, string>();
            _Cookies = new CookieCollection();
            _Handler = handler;
            _PostFields = new Dictionary<string, string>();
            _GetFields = new Dictionary<string, string>();
            _RedirectedFrom = new List<string>();
        }

        #region get-/setters

        /// <summary>
        /// The protocol supported by the client.
        /// </summary>
        public ProtocolType ProtocolType
        {
            get { return _ProtocolType; }
        }

        /// <summary>
        /// Check, whether this request has been redirected
        /// by a server rule.
        /// </summary>
        public bool Redirected
        {
            get
            {
                return _Redirected;
            }
        }

        /// <summary>
        /// This property tells you the original requested file,
        /// if the request has been redirected by the server.
        /// The request could have been redirected multiple times,
        /// so the return type of this property is a list with all
        /// previous URLs.
        /// </summary>
        public Collection<string> RedirectedFrom
        {
            get
            {
                return new Collection<string>(_RedirectedFrom);
            }
        }

        /// <summary>
        /// Specifies, whether the client is able to read compressed data (GZip).
        /// </summary>
        public bool CompressionAvailable
        {
            get { return this["accept-encoding"].ToLower().Contains("gzip"); }
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
        public string File
        {
            get
            {
                return _File;
            }
        }

        /// <summary>
        /// Retrieve a header field of the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field</param>
        /// <returns>The value of the requested header field</returns>
        public string this[string additionalHeader]
        {
            get
            {
                if (_Additional.ContainsKey(additionalHeader.ToLower())) return _Additional[additionalHeader.ToLower()];
                return "";
            }
        }

        /// <summary>
        /// The user agent of the client.
        /// </summary>
        public string UserAgent
        {
            get
            {
                if (_Additional.ContainsKey("user-agent")) return _Additional["user-agent"];
                return "";
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
        /// The project this request relates to.
        /// </summary>
        public IProject Project
        {
            get
            {
                return _Project;
            }
            set
            {
                _Project = value;
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
        public string GetParameter(string name)
        {
            if (PostFields.ContainsKey(name)) return PostFields[name];
            if (GetFields.ContainsKey(name)) return GetFields[name];
            return null;
        }

        /// <summary>
        /// The address (URI) of the resource from which the Request-URI was obtained.
        /// </summary>
        public string Referer
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
        public ClientHandler Handler
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
        public string Host
        {
            get
            {
                if (_Additional.ContainsKey("host")) return _Additional["host"];
                return null;
            }
        }

        /// <summary>
        /// Get or set, whether this request was redirected due to virtual hosting.
        /// </summary>
        public bool VirtualHosting
        {
            get { return _VirtualHosting; }
            set { _VirtualHosting = value; }
        }

        #endregion

        #region parser-support

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
            _File = URL;
            // rewrite the request if neccessary
            try
            {
                foreach (RewriteConfiguration rule in _Handler.Server.Configuration.GetRewrites())
                {
                    if (rule.Regex)
                    {
                        // dynamic rule
                        Regex from = new Regex(rule.Url);
                        string new_url = rule.To;
                        if (from.IsMatch(_File))
                        {
                            Match match = from.Match(_File);
                            for (int i = 1; i < match.Groups.Count; i++)
                            {
                                new_url = new_url.Replace("$" + i, match.Groups[i].Value);
                            }
                            Redirect(new_url);
                        }
                    }
                    else
                    {
                        // static rule
                        if (_File == rule.Url)
                        {
                            Redirect(rule.To);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _Handler.Server.Log.WriteLineColored("Failed to apply rewrite rules: " + e.Message, ConsoleColor.Red);
            }
            // find the matching project
            PatternProject proj = new PatternProject();
            if (proj.IsMatch(_File))
            {
                Match m = proj.Match(_File);
                try
                {
                    _Project = _Handler.Server.Projects[m.Groups[1].Value];
                }
                catch { }
            }
            // read GET parameters
            int pos = _File.IndexOf('?');
            if (pos > -1)
            {
                string getPart = (_File.Length > pos) ? _File.Substring(pos + 1) : "";
                PatternGetParameter get = new PatternGetParameter();
                foreach (Match m in get.Matches(getPart))
                {
                    // add this get parameter only, if it does not exist yet
                    if (!_GetFields.ContainsKey(m.Groups[1].Value))
                        _GetFields.Add(m.Groups[1].Value, Uri.UnescapeDataString(m.Groups[2].Value.Replace('+', ' ')));
                }
                Redirect(_File.Substring(0, pos));
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

        /// <summary>
        /// Get the name of a request type.
        /// </summary>
        /// <param name="type">The type to get the name to</param>
        /// <returns>The name of the given request type</returns>
        public static string GetRequestTypeName(RequestType type)
        {
            return Enum.GetName(typeof(RequestType), type);
        }

        /// <summary>
        /// Log this request to a log file.
        /// </summary>
        /// <param name="l">The log handler to write to</param>
        public void Log(Log l)
        {
            l.WriteLine(_Handler.IP + ":" + GetRequestTypeName(_Type) + " " + _Handler.Port + " - " + File);
        }

        /// <summary>
        /// Redirect this request.
        /// </summary>
        /// <param name="toURL">The new URL</param>
        public void Redirect(string toURL)
        {
            _RedirectedFrom.Add(_File);
            _File = toURL;
            _Redirected = true;
        }

        /// <summary>
        /// Allows you to normalize an url, if virtual hosting is used.
        /// </summary>
        /// <param name="url">The url to normalize</param>
        /// <returns>The normalized url</returns>
        /// <remarks>
        /// If the requested file lies on a virtual host, it does not need
        /// the name of the project in the url. You should use this method
        /// to calculate all of your urls.
        /// </remarks>
        public string NormalizeAbsoluteUrl(string url)
        {
            if (!url.StartsWith("/")) url = "/" + url;
            if (_Project != null)
            {
                if (_VirtualHosting) return url;
                return "/" + _Project.Name + url;
            }
            return url;
        }

    }

}
