using System;
using System.IO;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class RequestBuilder : IBuilder<IRequest>
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
            get { return _Cookies ??= new(); }
        }

        private ForwardingCollection Forwardings
        {
            get { return _Forwardings ??= new(); }
        }

        internal RequestHeaderCollection Headers { get; }

        #endregion

        #region Initialization

        internal RequestBuilder()
        {
            Headers = new();
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

        public RequestBuilder Protocol(HttpProtocol version)
        {
            _Protocol = version;
            return this;
        }

        public RequestBuilder Type(FlexibleRequestMethod type)
        {
            _RequestMethod = type;
            return this;
        }

        public RequestBuilder Path(WebPath path)
        {
            _Target = new(path);
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
                if (_Server is null)
                {
                    throw new BuilderMissingPropertyException("Server");
                }

                if (_EndPoint is null)
                {
                    throw new BuilderMissingPropertyException("EndPoint");
                }

                if (_Address is null)
                {
                    throw new BuilderMissingPropertyException("Address");
                }

                if (_Protocol is null)
                {
                    throw new BuilderMissingPropertyException("Protocol");
                }

                if (_RequestMethod is null)
                {
                    throw new BuilderMissingPropertyException("Type");
                }

                if (_Target is null)
                {
                    throw new BuilderMissingPropertyException("Target");
                }

                var protocol = (_EndPoint.Secure) ? ClientProtocol.HTTPS : ClientProtocol.HTTP;

                if (!Headers.TryGetValue("Host", out var host))
                {
                    throw new ProtocolException("Mandatory 'Host' header is missing from the request");
                }

                if (_Forwardings is null)
                {
                    Forwardings.TryAddLegacy(Headers);
                }

                var localClient = new ClientConnection(_Address, protocol, host);

                var client = DetermineClient() ?? localClient;

                return new Request(_Server, _EndPoint, client, localClient, (HttpProtocol)_Protocol, _RequestMethod, 
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

        private ClientConnection? DetermineClient()
        {
            if (_Forwardings is not null)
            {
                foreach (var forwarding in Forwardings)
                {
                    if (forwarding.For is not null)
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
