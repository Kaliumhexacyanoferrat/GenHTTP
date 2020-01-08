using System;
using System.Collections.Generic;
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

        #region Get-/Setters

        public IServer Server { get; }

        public IEndPoint EndPoint { get; }

        public IClientConnection Client { get; }

        public IClientConnection LocalClient { get; }

        public IRoutingContext? Routing { get; set; }

        public HttpProtocol ProtocolType { get; }

        public FlexibleRequestMethod Method { get; }

        public string Path { get; }

        public ICookieCollection Cookies { get; }

        public IForwardingCollection Forwardings { get; }

        public IHeaderCollection Headers { get; }

        public IReadOnlyDictionary<string, string> Query { get; }

        public Stream? Content { get; }

        public string? Host => Client.Host;

        public string? Referer => this["Referer"];

        public string? UserAgent => this["User-Agent"];

        public string? this[string additionalHeader]
        {
            get
            {
                if (Headers.ContainsKey(additionalHeader.ToLower()))
                {
                    return Headers[additionalHeader.ToLower()];
                }

                return null;
            }
        }

        #endregion

        #region Initialization

        internal Request(IServer server, IEndPoint endPoint, IClientConnection client, IClientConnection localClient, HttpProtocol protocol, FlexibleRequestMethod method,
                         string path, IHeaderCollection headers, ICookieCollection cookies, IForwardingCollection forwardings,
                         IReadOnlyDictionary<string, string> query, Stream? content)
        {
            Client = client;
            LocalClient = localClient;

            Server = server;
            EndPoint = endPoint;

            ProtocolType = protocol;
            Method = method;
            Path = path;

            Cookies = cookies;
            Forwardings = forwardings;
            Headers = headers;
            Query = query;

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
                    if (Content != null)
                    {
                        Content.Dispose();
                    }
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
