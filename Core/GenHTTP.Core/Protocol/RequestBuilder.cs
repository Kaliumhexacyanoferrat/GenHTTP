using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class RequestBuilder : IBuilder<IRequest>
    {
        private IServer? _Server;
        private IClient? _Client;
        private IEndPoint? _EndPoint;

        private FlexibleRequestMethod? _RequestMethod;
        private ProtocolType? _Protocol;

        private string? _Path;

        private Stream? _Content;

        private readonly Dictionary<string, string> _Query = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private readonly CookieCollection _Cookies = new CookieCollection();

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

        public RequestBuilder Connection(IServer server, IEndPoint endPoint, IClient client)
        {
            _Server = server;
            _Client = client;
            _EndPoint = endPoint;

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
            _RequestMethod = new FlexibleRequestMethod(type);
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

                if (_Query.Count == 0)
                {
                    _Query[query] = string.Empty;
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
                Headers[key] = value.Trim();
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
            if (_Server == null)
            {
                throw new BuilderMissingPropertyException("Server");
            }

            if (_EndPoint == null)
            {
                throw new BuilderMissingPropertyException("EndPoint");
            }

            if (_Client == null)
            {
                throw new BuilderMissingPropertyException("Client");
            }

            if (_Protocol == null)
            {
                throw new BuilderMissingPropertyException("Protocol");
            }

            if (_RequestMethod is null)
            {
                throw new BuilderMissingPropertyException("Type");
            }

            if (_Path == null)
            {
                throw new BuilderMissingPropertyException("Path");
            }

            return new Request(_Server, _EndPoint, _Client, (ProtocolType)_Protocol, _RequestMethod, _Path, Headers, _Cookies, _Query, _Content);
        }

        #endregion

    }

}
