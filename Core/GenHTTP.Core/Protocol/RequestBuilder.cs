using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class RequestBuilder : IBuilder<IRequest>
    {
        private IClientHandler? _ClientHandler;

        private RequestType? _RequestType;
        private ProtocolType? _Protocol;

        private string? _Path;

        private Stream? _Content;

        private Dictionary<string, string> _Query = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private CookieCollection _Cookies = new CookieCollection();

        #region Get-/Setters

        internal HeaderCollection Headers { get; }

        #endregion

        #region Initialization

        internal RequestBuilder()
        {
            Headers = new HeaderCollection();
        }

        #endregion

        #region Functionality

        public RequestBuilder Handler(IClientHandler clientHandler)
        {
            _ClientHandler = clientHandler;
            return this;
        }

        public RequestBuilder Protocol(string version)
        {
            switch (version)
            {
                case "1.0":
                    {
                        _Protocol = ProtocolType.Http_1_0;
                        break;
                    }
                case "1.1":
                    {
                        _Protocol = ProtocolType.Http_1_1;
                        break;
                    }
                default:
                    {
                        throw new ProtocolException($"HTTP version '{version}' is not supported");
                    }
            }

            return this;
        }

        public RequestBuilder Type(string type)
        {
            if (Enum.TryParse<RequestType>(type.ToUpper(), out var parsed))
            {
                _RequestType = parsed;
            }
            else
            {
                throw new ProtocolException($"HTTP verb '{type}' is not supported");
            }

            return this;
        }

        public RequestBuilder Path(string path)
        {
            var index = path.IndexOf('?');

            if (index > -1)
            {
                _Path = path.Substring(0, index);

                var query = (path.Length > index) ? path.Substring(index + 1) : "";

                foreach (Match m in Pattern.GET_PARAMETER.Matches(query))
                {
                    _Query[m.Groups[1].Value] = Uri.UnescapeDataString(m.Groups[2].Value.Replace('+', ' '));
                }
            }
            else
            {
                _Path = path;
            }

            return this;
        }

        public RequestBuilder Header(string key, string value)
        {
            if (key.ToLower() == "cookie")
            {
                var cookies = value.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var kv in cookies)
                {
                    var index = kv.IndexOf("=");

                    if (index > -1)
                    {
                        var cookie = new Cookie(kv.Substring(0, index), kv.Substring(index + 1));
                        _Cookies[cookie.Name] = cookie;
                    }
                }
            }
            else
            {
                Headers[key] = value;
            }

            return this;
        }
        
        public RequestBuilder Content(Stream content)
        {
            _Content = content;
            return this;
        }

        public IRequest Build()
        {
            if (_ClientHandler == null)
            {
                throw new BuilderMissingPropertyException("Handler");
            }

            if (_Protocol == null)
            {
                throw new BuilderMissingPropertyException("Protocol");
            }

            if (_RequestType == null)
            {
                throw new BuilderMissingPropertyException("Type");
            }

            if (_Path == null)
            {
                throw new BuilderMissingPropertyException("Path");
            }

            return new Request((ProtocolType)_Protocol, (RequestType)_RequestType, _Path, Headers, _Cookies, _Query, _Content, _ClientHandler);
        }

        #endregion

    }

}
