﻿using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Allows to configure and create a new <see cref="IServer" /> instance.
/// </summary>
public interface IServerBuilder : IServerBuilder<IServerBuilder>;

/// <summary>
/// Allows to configure and create a new <see cref="IServer" /> instance.
/// </summary>
public interface IServerBuilder<out T> : IBuilder<IServer>
{

    #region Binding

    /// <summary>
    /// Specifies the port, the server will listen on (defaults to 8080 on IPv4/IPv6).
    /// </summary>
    /// <param name="port">The port the server should listen on</param>
    /// <remarks>
    /// If you register custom endpoints using the Bind methods, this value
    /// will be ignored.
    /// </remarks>
    T Port(ushort port);

    /// <summary>
    /// Registers an endpoint for the given address and port the server will
    /// bind to on startup to listen for incomming HTTP requests.
    /// </summary>
    /// <param name="address">The address to bind to (or null, if the server should listen to any IP)</param>
    /// <param name="port">The port to listen on</param>
    T Bind(IPAddress? address, ushort port);

    /// <summary>
    /// Registers a secure endpoint the server will bind to on
    /// startup to listen for incoming HTTPS requests.
    /// </summary>
    /// <param name="address">The address to bind to (or null, if the server should listen to any IP)</param>
    /// <param name="port">The port to listen on</param>
    /// <param name="certificate">The certificate used to negoiate a connection with</param>
    /// <param name="protocols">The SSL/TLS protocl versions which should be supported by the endpoint</param>
    /// <param name="certificateValidator">The validator to check the validity of client certificates with</param>
    /// <param name="enableQuic">If enabled, the server will host a HTTP/3 endpoint via QUIC</param>
    T Bind(IPAddress? address, ushort port, X509Certificate2 certificate, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false);

    /// <summary>
    /// Registers a secure endpoint the server will bind to on
    /// startup to listen for incoming HTTPS requests.
    /// </summary>
    /// <param name="address">The address to bind to (or null, if the server should listen to any IP)</param>
    /// <param name="port">The port to listen on</param>
    /// <param name="certificateProvider">The provider to select the certificate used to negoiate a connection with</param>
    /// <param name="protocols">The SSL/TLS protocl versions which should be supported by the endpoint</param>
    /// <param name="certificateValidator">The validator to check the validity of client certificates with</param>
    /// <param name="enableQuic">If enabled, the server will host a HTTP/3 endpoint via QUIC</param>
    T Bind(IPAddress? address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false);

    #endregion

    #region Infrastructure

    /// <summary>
    /// Registers a companion that will log all handled requests and
    /// errors to the console.
    /// </summary>
    T Console();

    /// <summary>
    /// Registers the given companion to be used by the server, allowing
    /// to log and handle requests and errors.
    /// </summary>
    /// <param name="companion">The companion to be used by the server</param>
    T Companion(IServerCompanion companion);

    /// <summary>
    /// Enables or disables the development mode on the server instance. When in
    /// development mode, the server will return additional information
    /// useful for developers of web applications.
    /// </summary>
    /// <param name="developmentMode">Whether the development should be active</param>
    /// <remarks>
    /// By default, the development mode is disabled.
    /// </remarks>
    T Development(bool developmentMode = true);

    #endregion

    #region Network settings

    /// <summary>
    /// Configures the number of connections the operating system will accept
    /// while they not have yet been accepted by the server.
    /// </summary>
    /// <param name="backlog">The number of connections to be accepted</param>
    /// <remarks>
    /// Adjust this value only, if you expect large bursts of simultaneous requests
    /// or your server requires very long to generate requests.
    /// </remarks>
    T Backlog(ushort backlog);

    /// <summary>
    /// Specifies the period of time after which the server will
    /// assume the client connection timed out.
    /// </summary>
    T RequestReadTimeout(TimeSpan timeout);

    /// <summary>
    /// Requests smaller than this limit (in bytes) will be held in memory, while
    /// larger requests will be cached in a temporary file.
    /// </summary>
    T RequestMemoryLimit(uint limit);

    /// <summary>
    /// Size of the buffer that will be used to read or write large
    /// data streams (such as uploads or downloads).
    /// </summary>
    T TransferBufferSize(uint bufferSize);

    #endregion

    #region Content

    /// <summary>
    /// Specifies the root handler that will be invoked when
    /// a client request needs to be handled.
    /// </summary>
    /// <param name="handler">The handler to be invoked to handle requests</param>
    /// <remarks>
    /// Note that only a single handler is supported. To build are more
    /// complex application, consider passing a Layout instead.
    /// </remarks>
    T Handler(IHandler handler);

    /// <summary>
    /// Specifies the root handler that will be invoked when
    /// a client request needs to be handled.
    /// </summary>
    /// <param name="handler">The handler to be invoked to handle requests</param>
    /// <remarks>
    /// Note that only a single handler is supported. To build are more
    /// complex application, consider passing a Layout instead.
    /// </remarks>
    T Handler(IHandlerBuilder handlerBuilder) => Handler(handlerBuilder.Build());

    /// <summary>
    /// Adds a concern to the server instance which will be executed before
    /// and after the root handler is invoked.
    /// </summary>
    /// <param name="concern">The concern to be added to the instance</param>
    T Add(IConcernBuilder concern);

    #endregion

}
