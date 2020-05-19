using System;
using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    /// <summary>
    /// Provides methods to access a recieved http request.
    /// </summary>
    internal class Request : IRequest
    {
        private ICookieCollection? _Cookies;

        private IForwardingCollection? _Forwardings;

        private IRequestQuery? _Query;

        private IRequestProperties? _Properties;

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
            get { return _Cookies ?? (_Cookies = new CookieCollection()); }
        }

        public IForwardingCollection Forwardings
        {
            get { return _Forwardings ?? (_Forwardings = new ForwardingCollection()); }
        }

        public IRequestQuery Query
        {
            get { return _Query ?? (_Query = new RequestQuery()); }
        }

        public IRequestProperties Properties
        {
            get { return _Properties ?? (_Properties = new RequestProperties()); }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Headers.Dispose();

                    _Query?.Dispose();

                    _Cookies?.Dispose();

                    Content?.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
