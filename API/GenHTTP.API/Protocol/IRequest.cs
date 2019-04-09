﻿using System;
using System.Collections.Generic;
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
        IClient Client { get; }

        /// <summary>
        /// The routing context of the request, if applicable.
        /// </summary>
        IRoutingContext? Routing { get; set; }

        #endregion

        #region HTTP Protocol

        /// <summary>
        /// The requested protocol type.
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// The HTTP method used by the client to issue this request.
        /// </summary>
        FlexibleRequestMethod Method { get; }

        /// <summary>
        /// The path requested by the client (with no query parameters attached).
        /// </summary>
        string Path { get; }

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
        IReadOnlyDictionary<string, string> Query { get; }

        /// <summary>
        /// The cookies passed by the client.
        /// </summary>
        ICookieCollection Cookies { get; }

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