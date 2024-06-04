﻿using System;
using System.Net;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Infrastructure
{

    #region Error scopes

    /// <summary>
    /// The kind of errors which may occur within the
    /// server engine.
    /// </summary>
    public enum ServerErrorScope
    {

        /// <summary>
        /// Errors which occur within the regular lifecycle, 
        /// such as startup errors.
        /// </summary>
        General,

        /// <summary>
        /// Errors which occur when listening for requests or
        /// when handling them.
        /// </summary>
        ServerConnection,

        /// <summary>
        /// Errors which occur when communicating with the client,
        /// such as aborted connections.
        /// </summary>
        ClientConnection,

        /// <summary>
        /// Errors which occur when trying to establish a secure
        /// connection with the client.
        /// </summary>
        Security,

        /// <summary>
        /// Errors which occur when the server tries to generate a default
        /// error page, e.g. because the template somehow fails to render.
        /// </summary>
        PageGeneration,

        /// <summary>
        /// An error which occurred within an extension.
        /// </summary>
        Extension,

    }

    #endregion

    /// <summary>
    /// A companion which can be registered at the server to handle
    /// requests and error messages.
    /// </summary>
    /// <remarks>
    /// Bad runtime characteristics of an implementing class can severly
    /// lower the throughput of your server instance. If you would like to
    /// perform long-running tasks such as logging to a database, it's recommended
    /// to add some kind of asynchronous worker mechanism.
    /// </remarks>
    public interface IServerCompanion
    {

        /// <summary>
        /// Will be invoked after request has been handled by the server.
        /// </summary>
        /// <param name="request">The request which has been handled</param>
        /// <param name="response">The response which has been generated by the server</param>
        void OnRequestHandled(IRequest request, IResponse response);

        /// <summary>
        /// Will be invoked if an error occurred within the server engine.
        /// </summary>
        /// <param name="scope">The scope of the error</param>
        /// <param name="client">The endpoint of the client which caused this error (if any)</param>
        /// <param name="error">The actual exception which occurred</param>
        void OnServerError(ServerErrorScope scope, IPAddress? client, Exception error);

    }

}
