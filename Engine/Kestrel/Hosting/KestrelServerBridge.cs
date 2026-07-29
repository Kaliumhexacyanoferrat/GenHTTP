using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenHTTP.Engine.Kestrel.Hosting;

internal sealed class KestrelServerBridge : IServer
{

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; private set; }

    public bool Development { get; }

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => Configuration.Logging;

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }

    private ServerConfiguration Configuration { get; }

    private KestrelServer Instance { get; }

    #endregion

    #region Initialization

    internal KestrelServerBridge(ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Configuration = configuration;

        Development = configuration.DevelopmentMode;

        Handler = handler;

        var endpoints = new KestrelEndpoints();

        endpoints.AddRange(configuration.EndPoints.Select(e => new KestrelEndpoint(e.Address, e.Port, e.DualStack, e.Security is not null)));

        EndPoints = endpoints;

        Instance = Spawn();
    }

    #endregion

    #region Functionality

    private KestrelServer Spawn()
    {
        var options = Configure();

        var transportFactory = new SocketTransportFactory(Options.Create(new SocketTransportOptions()), Configuration.Logging);

        return new KestrelServer(options, transportFactory, Configuration.Logging);
    }

    public async ValueTask StartAsync()
    {
        await Handler.PrepareAsync(this);

        await Instance.StartAsync(new Application(this), CancellationToken.None);

        Running = true;

        // Other engines log this from their own endpoint-binding code (see e.g. Internal's
        // EndPoint.Start()). Kestrel would normally get this from the generic host's
        // "Now listening on ..." lifecycle logging, but we bypass that host layer entirely
        // by driving KestrelServer directly, so nothing would otherwise log it.
        var logger = Logging.CreateLogger<KestrelServerBridge>();

        foreach (var endpoint in EndPoints)
        {
            logger.LogInformation("Listening on {Address}:{Port} ({Settings})", endpoint.Address, endpoint.Port, endpoint.Secure ? "HTTPS" : "HTTP");
        }
    }

    private IOptions<KestrelServerOptions> Configure()
    {
        var builder = Options.Create(new KestrelServerOptions());

        var options = builder.Value;

        // Every KestrelServerOptions.UseHttps(...) overload routes through
        // EnableHttpsConfiguration(), which unconditionally resolves an
        // IHttpsConfigurationService from options.ApplicationServices - normally supplied by
        // the generic host's DI container. We drive KestrelServer directly and have no such
        // container, so build the smallest one that satisfies it.
        options.ApplicationServices = BuildApplicationServices();

        options.AllowSynchronousIO = true;

        options.Limits.MaxRequestBodySize = null;

        // ResponseWriter sets its own "Server: GenHTTP/x.y" header - don't let
        // Kestrel add its own "Server: Kestrel" alongside it.
        options.AddServerHeader = false;

        foreach (var endpoint in Configuration.EndPoints)
        {
            if ((endpoint.Address == null) || (endpoint.DualStack && (endpoint.Address.Equals(IPAddress.Any) || endpoint.Address.Equals(IPAddress.IPv6Any))))
            {
                options.ListenAnyIP(endpoint.Port, listenOptions =>
                {
                    if (endpoint.Security is not null)
                    {
                        Secure(listenOptions, endpoint, endpoint.Security);
                    }
                });
            }
            else
            {
                options.Listen(endpoint.Address, endpoint.Port, listenOptions =>
                {
                    if (endpoint.Security is not null)
                    {
                        Secure(listenOptions, endpoint, endpoint.Security);
                    }
                });
            }
        }

        return builder;
    }

    /// <summary>
    /// <see cref="Microsoft.AspNetCore.Server.Kestrel.Core.IHttpsConfigurationService"/> and
    /// <c>KestrelMetrics</c> (both required to activate <c>UseHttps(...)</c>, see
    /// <see cref="Secure"/>) are internal to Kestrel's assembly - there is no supported public
    /// way to construct them when not going through the generic host's DI container, so this
    /// resolves them by name via reflection. Fragile in principle (an internal implementation
    /// detail we depend on), but empirically the minimal container that satisfies
    /// EnableHttpsConfiguration() without pulling in the full ASP.NET Core hosting stack.
    /// </summary>
    private IServiceProvider BuildApplicationServices()
    {
        var kestrelCore = typeof(KestrelServerOptions).Assembly;

        var httpsConfigurationServiceInterface = kestrelCore.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.IHttpsConfigurationService", throwOnError: true)!;
        var httpsConfigurationService = kestrelCore.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.HttpsConfigurationService", throwOnError: true)!;
        var kestrelMetrics = kestrelCore.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.KestrelMetrics", throwOnError: true)!;

        var services = new ServiceCollection();

        services.AddSingleton(Configuration.Logging);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddMetrics();

        services.AddSingleton(httpsConfigurationServiceInterface, httpsConfigurationService);
        services.AddSingleton(kestrelMetrics);

        return services.BuildServiceProvider();
    }

    private static void Secure(ListenOptions options, EndPointConfiguration endpoint, SecurityConfiguration security)
    {
        options.Protocols = (endpoint.EnableQuic) ? HttpProtocols.Http1AndHttp2AndHttp3 : HttpProtocols.Http1AndHttp2;

        // Every UseHttps(...) overload routes through KestrelServerOptions.
        // EnableHttpsConfiguration(), which resolves services from options.ApplicationServices
        // regardless of which overload is used - see BuildApplicationServices().
        var httpsOptions = new HttpsConnectionAdapterOptions
        {
            SslProtocols = security.Protocols,
            ServerCertificateSelector = (_, hostName) => security.CertificateProvider.Provide(hostName)
        };

        var validator = security.CertificateValidator;

        if (validator != null)
        {
            httpsOptions.ClientCertificateMode = validator.RequireCertificate ? ClientCertificateMode.RequireCertificate : ClientCertificateMode.AllowCertificate;
            httpsOptions.ClientCertificateValidation = validator.Validate;
            httpsOptions.CheckCertificateRevocation = (validator.RevocationCheck != X509RevocationMode.NoCheck);
        }

        options.UseHttps(httpsOptions);
    }

    #endregion

    #region Lifecycle

    private bool _disposed;

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await Instance.StopAsync(CancellationToken.None);

            Instance.Dispose();

            _disposed = true;
        }
    }

    #endregion

}
