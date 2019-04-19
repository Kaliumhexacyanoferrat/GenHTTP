using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Allows to configure and create a new <see cref="IServer"/> instance.
    /// </summary>
    public interface IServerBuilder : IBuilder<IServer>
    {

        #region Content

        /// <summary>
        /// Registers the router which will be used to handle all requests.
        /// </summary>
        /// <param name="routerBuilder">The router to be registered</param>
        IServerBuilder Router(IRouterBuilder routerBuilder);

        /// <summary>
        /// Registers the router which will be used to handle all requests.
        /// </summary>
        /// <param name="routerBuilder">The router to be registered</param>
        IServerBuilder Router(IRouter router);

        #endregion

        #region Infrastructure
        
        /// <summary>
        /// Registers a companion that will log all handled requests and
        /// errors to the console.
        /// </summary>
        IServerBuilder Console();

        /// <summary>
        /// Registers the given companion to be used by the server, allowing
        /// to log and handle requests and errors.
        /// </summary>
        /// <param name="companion">The companion to be used by the server</param>
        IServerBuilder Companion(IServerCompanion companion);

        /// <summary>
        /// Registers the given extension in the server.
        /// </summary>
        /// <param name="extension">The extension to be registerd</param>
        IServerBuilder Extension(IServerExtensionBuilder extension);

        /// <summary>
        /// Registers the given extension in the server.
        /// </summary>
        /// <param name="extension">The extension to be registerd</param>
        IServerBuilder Extension(IServerExtension extension);

        /// <summary>
        /// Enables or disables the development mode on the server instance. When in
        /// development mode, the server will return additional information
        /// useful for developers of web applications.
        /// </summary>
        /// <param name="developmentMode">Whether the development should be active</param>
        /// <remarks>
        /// By default, the development mode is disabled.
        /// </remarks>
        IServerBuilder Development(bool developmentMode = true);

        #endregion

        #region Compression

        /// <summary>
        /// Registers the given compression algorithm to be used by the server.
        /// </summary>
        /// <param name="algorithm">The algorithm to be registered</param>
        IServerBuilder Compression(IBuilder<ICompressionAlgorithm> algorithm);

        /// <summary>
        /// Registers the given compression algorithm to be used by the server.
        /// </summary>
        /// <param name="algorithm">The algorithm to be registered</param>
        IServerBuilder Compression(ICompressionAlgorithm algorithm);

        /// <summary>
        /// Enables or disables compression for this server instance. By default,
        /// the server will compress content using the gzip algorithm, if applicable.
        /// </summary>
        /// <param name="enabled">Whether compression is enabled for this server</param>
        IServerBuilder Compression(bool enabled);

        #endregion

        #region Binding

        /// <summary>
        /// Specifies the port, the server will listen on (defaults to 8080 on IPv4/IPv6).
        /// </summary>
        /// <param name="port">The port the server should listen on</param>
        /// <remarks>
        /// If you register custom endpoints using the Bind methods, this value
        /// will be ignored.
        /// </remarks>
        IServerBuilder Port(ushort port);
        
        /// <summary>
        /// Registers an endpoint for the given address and port the server will
        /// bind to on startup to listen for incomming HTTP requests.
        /// </summary>
        /// <param name="address">The address to bind to</param>
        /// <param name="port">The port to listen on</param>
        IServerBuilder Bind(IPAddress address, ushort port);
        
        /// <summary>
        /// Registers a secure endpoint the server will bind to on
        /// startup to listen for incoming HTTPS requests.
        /// </summary>
        /// <param name="address">The address to bind to</param>
        /// <param name="port">The port to listen on</param>
        /// <param name="certificate">The certificate used to negoiate a connection with</param>
        /// <remarks>
        /// By default, the endpoint will accept TLS 1.2 connections only.
        /// </remarks>
        IServerBuilder Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate);

        /// <summary>
        /// Registers a secure endpoint the server will bind to on
        /// startup to listen for incoming HTTPS requests.
        /// </summary>
        /// <param name="address">The address to bind to</param>
        /// <param name="port">The port to listen on</param>
        /// <param name="certificate">The certificate used to negoiate a connection with</param>
        /// <param name="protocols">The SSL/TLS protocl versions which should be supported by the endpoint</param>
        IServerBuilder Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate, SslProtocols protocols);

        /// <summary>
        /// Registers a secure endpoint the server will bind to on
        /// startup to listen for incoming HTTPS requests.
        /// </summary>
        /// <param name="address">The address to bind to</param>
        /// <param name="port">The port to listen on</param>
        /// <param name="certificateProvider">The provider to select the certificate used to negoiate a connection with</param>
        /// <remarks>
        /// By default, the endpoint will accept TLS 1.2 connections only.
        /// </remarks>
        IServerBuilder Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider);

        /// <summary>
        /// Registers a secure endpoint the server will bind to on
        /// startup to listen for incoming HTTPS requests.
        /// </summary>
        /// <param name="address">The address to bind to</param>
        /// <param name="port">The port to listen on</param>
        /// <param name="certificateProvider">The provider to select the certificate used to negoiate a connection with</param>
        /// <param name="protocols">The SSL/TLS protocl versions which should be supported by the endpoint</param>
        IServerBuilder Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols);

        #endregion

        #region Security

        /// <summary>
        /// Specifies the upgrade mode used to upgrade insecure requests
        /// to HTTPS secured endpoints.
        /// </summary>
        /// <param name="upgradeMode">The upgrade mode to be used</param>
        IServerBuilder SecureUpgrade(SecureUpgrade upgradeMode);

        /// <summary>
        /// Configures the HSTS to be applied by the server when serving content
        /// over secure endpoints.
        /// </summary>
        /// <param name="maximumAge">The maximum age of this policy</param>
        /// <param name="includeSubdomains">Whether subdomains are included in the policy or not</param>
        /// <param name="preload">Whether the policy is allowed to be preloaded</param>
        IServerBuilder StrictTransport(TimeSpan maximumAge, bool includeSubdomains = true, bool preload = true);

        /// <summary>
        /// Enables or disables the strict transport policy (HSTS), which is
        /// enabled by default.
        /// </summary>
        /// <param name="enabled">Whether the strict transport policy is enabled</param>
        IServerBuilder StrictTransport(bool enabled);

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
        IServerBuilder Backlog(ushort backlog);

        /// <summary>
        /// Specifies the period of time after which the server will
        /// assume the client connection timed out.
        /// </summary>
        IServerBuilder RequestReadTimeout(TimeSpan timeout);

        /// <summary>
        /// Requests smaller than this limit will be held in memory, while
        /// larger requests will be cached in a temporary file.
        /// </summary>
        IServerBuilder RequestMemoryLimit(uint limit);

        /// <summary>
        /// Size of the buffer that will be used to read or write large
        /// data streams (such as uploads or downloads).
        /// </summary>
        IServerBuilder TransferBufferSize(uint bufferSize);

        #endregion

    }
    
}
