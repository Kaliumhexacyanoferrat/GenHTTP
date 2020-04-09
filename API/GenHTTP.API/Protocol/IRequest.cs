﻿using System;
using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// A request send by the currently connected client.
    /// </summary>
    public interface IRequest : IDisposable
    {

        #region General Infrastructure

        /// <summary>
        /// The server handling the request.
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// The endpoint the request originates from.
        /// </summary>
        IEndPoint EndPoint { get; }

        /// <summary>
        /// The client which sent the request.
        /// </summary>
        IClientConnection Client { get; }

        /// <summary>
        /// If the request has been forwarded by a proxy, the client property
        /// will return the originating client where this property will return
        /// the information of the proxy.
        /// </summary>
        IClientConnection LocalClient { get; }

        #endregion

        #region HTTP Protocol

        /// <summary>
        /// The requested protocol type.
        /// </summary>
        HttpProtocol ProtocolType { get; }

        /// <summary>
        /// The HTTP method used by the client to issue this request.
        /// </summary>
        FlexibleRequestMethod Method { get; }

        /// <summary>
        /// The path requested by the client (with no query parameters attached).
        /// </summary>
        RoutingTarget Target { get; }

        #endregion

        #region Headers

        /// <summary>
        /// The user agent which issued this request, if any.
        /// </summary>
        string? UserAgent { get; }

        /// <summary>
        /// The referrer which caused the invociation of this request, if any. 
        /// </summary>
        string? Referer { get; }

        /// <summary>
        /// The host requested by the client, if any.
        /// </summary>
        string? Host { get; }

        /// <summary>
        /// Read an additional header value from the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field to be read</param>
        /// <returns>The value of the header field, if specified by the client</returns>
        string? this[string additionalHeader] { get; }

        /// <summary>
        /// The query parameters passed by the client.
        /// </summary>
        IRequestQuery Query { get; }

        /// <summary>
        /// The cookies passed by the client.
        /// </summary>
        ICookieCollection Cookies { get; }

        /// <summary>
        /// If the request has been forwarded by one or more proxies, this collection may contain
        /// additional information about the initial request by the originating client. 
        /// </summary>
        IForwardingCollection Forwardings { get; }

        /// <summary>
        /// The headers of this HTTP request.
        /// </summary>
        IHeaderCollection Headers { get; }

        #endregion

        #region Body

        /// <summary>
        /// The content transmitted by the client, if any.
        /// </summary>
        Stream? Content { get; }

        #endregion

        #region Functionality

        /// <summary>
        /// Generates a new response for this request to be send to the client.
        /// </summary>
        /// <returns>The newly created response</returns>
        IResponseBuilder Respond();

        #endregion

    }

}
