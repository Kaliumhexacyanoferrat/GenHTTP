using System;
using System.IO;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Engine.Protocol
{

    internal class RequestBuilder : IBuilder<IRequest>
    {
        private IServer? _Server;
        private IEndPoint? _EndPoint;

        private IPAddress? _Address;

        private FlexibleRequestMethod? _RequestMethod;
        private HttpProtocol? _Protocol;

        private RoutingTarget? _Target;

        private Stream? _Content;

        private RequestQuery? _Query;

        private CookieCollection? _Cookies;

        private ForwardingCollection? _Forwardings;

        #region Get-/Setters

        private CookieCollection Cookies
        {
            get { return _Cookies ?? (_Cookies = new CookieCollection()); }
        }

        private ForwardingCollection Forwardings
        {
            get { return _Forwardings ?? (_Forwardings = new ForwardingCollection()); }
        }

        internal HeaderCollection Headers { get; }

        #endregion

        #region Initialization

        internal RequestBuilder()
        {
            Headers = new HeaderCollection();
        }

        #endregion

        #region Functionality

        public RequestBuilder Connection(IServer server, IEndPoint endPoint, IPAddress? address)
        {
            _Server = server;
            _Address = address;
            _EndPoint = endPoint;

            return this;
        }

        public RequestBuilder Protocol(string version)
        {
            switch (version)
            {
                case "1.0":
                    {
                        _Protocol = HttpProtocol.Http_1_0;
                        break;
                    }
                case "1.1":
                    {
                        _Protocol = HttpProtocol.Http_1_1;
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

        public RequestBuilder Path(WebPath path)
        {
            _Target = new RoutingTarget(path);
            return this;
        }

        public RequestBuilder Query(RequestQuery query)
        {
            _Query = query;
            return this;
        }

        public RequestBuilder Header(string key, string value)
        {
            if (string.Equals(key, "cookie", StringComparison.OrdinalIgnoreCase))
            {
                Cookies.Add(value);
            }
            else if (string.Equals(key, "forwarded", StringComparison.OrdinalIgnoreCase))
            {
                Forwardings.Add(value);
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
            try
            {
                if (_Server == null)
                {
                    throw new BuilderMissingPropertyException("Server");
                }

                if (_EndPoint == null)
                {
                    throw new BuilderMissingPropertyException("EndPoint");
                }

                if (_Address == null)
                {
                    throw new BuilderMissingPropertyException("Address");
                }

                if (_Protocol == null)
                {
                    throw new BuilderMissingPropertyException("Protocol");
                }

                if (_RequestMethod is null)
                {
                    throw new BuilderMissingPropertyException("Type");
                }

                if (_Target == null)
                {
                    throw new BuilderMissingPropertyException("Target");
                }

                var protocol = (_EndPoint.Secure) ? ClientProtocol.HTTPS : ClientProtocol.HTTP;

                if (!Headers.ContainsKey("Host"))
                {
                    throw new ProtocolException("Mandatory 'Host' header is missing from the request");
                }

                var localClient = new ClientConnection(_Address, protocol, Headers["Host"]);

                var client = DetermineClient() ?? localClient;

                return new Request(_Server, _EndPoint, client, localClient, (HttpProtocol)_Protocol, _RequestMethod.Value, 
                                   _Target, Headers, _Cookies, _Forwardings, _Query, _Content);
            }
            catch (Exception)
            {
                Headers.Dispose();

                _Query?.Dispose();
                _Cookies?.Dispose();

                throw;
            }
        }

        private IClientConnection? DetermineClient()
        {
            if (_Forwardings != null)
            {
                foreach (var forwarding in Forwardings)
                {
                    if (forwarding.For != null)
                    {
                        return new ClientConnection(forwarding.For, forwarding.Protocol, forwarding.Host);
                    }
                }
            }

            return null;
        }

        #endregion

    }

}
