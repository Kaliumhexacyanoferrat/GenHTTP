using System;
using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Protocol;

namespace GenHTTP.Engine
{

    /// <summary>
    /// Provides methods to access a recieved http request.
    /// </summary>
    internal sealed class Request : IRequest
    {
        private ICookieCollection? _Cookies;

        private IForwardingCollection? _Forwardings;

        private IRequestQuery? _Query;

        private IRequestProperties? _Properties;

        private FlexibleContentType? _ContentType;

        #region Get-/Setters

        public IServer Server { get; }

        public IEndPoint EndPoint { get; }

        public IClientConnection Client { get; }

        public IClientConnection LocalClient { get; }

        public HttpProtocol ProtocolType { get; }

        public FlexibleRequestMethod Method { get; }

        public RoutingTarget Target { get; }

        public IHeaderCollection Headers { get; }

        public Stream? Content { get; }

        public FlexibleContentType? ContentType
        {
            get
            {
                if (_ContentType is not null)
                {
                    return _ContentType;
                }

                var type = this["Content-Type"];

                if (type is not null)
                {
                    return _ContentType = new(type);
                }

                return null;
            }
        }

        public string? Host => Client.Host;

        public string? Referer => this["Referer"];

        public string? UserAgent => this["User-Agent"];

        public string? this[string additionalHeader]
        {
            get
            {
                if (Headers.ContainsKey(additionalHeader))
                {
                    return Headers[additionalHeader];
                }

                return null;
            }
        }

        public ICookieCollection Cookies
        {
            get { return _Cookies ??= new CookieCollection(); }
        }

        public IForwardingCollection Forwardings
        {
            get { return _Forwardings ??= new ForwardingCollection(); }
        }

        public IRequestQuery Query
        {
            get { return _Query ??= new RequestQuery(); }
        }

        public IRequestProperties Properties
        {
            get { return _Properties ??= new RequestProperties(); }
        }

        #endregion

        #region Initialization

        internal Request(IServer server, IEndPoint endPoint, IClientConnection client, IClientConnection localClient, HttpProtocol protocol, FlexibleRequestMethod method,
                         RoutingTarget target, IHeaderCollection headers, ICookieCollection? cookies, IForwardingCollection? forwardings,
                         IRequestQuery? query, Stream? content)
        {
            Client = client;
            LocalClient = localClient;

            Server = server;
            EndPoint = endPoint;

            ProtocolType = protocol;
            Method = method;
            Target = target;

            _Cookies = cookies;
            _Forwardings = forwardings;
            _Query = query;

            Headers = headers;

            Content = content;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Respond()
        {
            return new ResponseBuilder(this).Status(ResponseStatus.OK);
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                Headers.Dispose();

                _Query?.Dispose();

                _Cookies?.Dispose();

                _Properties?.Dispose();

                Content?.Dispose();

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
