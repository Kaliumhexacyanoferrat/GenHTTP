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
    }

    private IOptions<KestrelServerOptions> Configure()
    {
        var builder = Options.Create(new KestrelServerOptions());

        var options = builder.Value;

        options.AllowSynchronousIO = true;

        options.Limits.MaxRequestBodySize = null;

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

    private static void Secure(ListenOptions options, EndPointConfiguration endpoint, SecurityConfiguration security)
    {
        options.Protocols = (endpoint.EnableQuic) ? HttpProtocols.Http1AndHttp2AndHttp3 : HttpProtocols.Http1AndHttp2;

        options.UseHttps(httpsOptions =>
        {
            httpsOptions.SslProtocols = security.Protocols;
            httpsOptions.ServerCertificateSelector = (_, hostName) => security.CertificateProvider.Provide(hostName);

            var validator = security.CertificateValidator;

            if (validator != null)
            {
                httpsOptions.ClientCertificateMode = validator.RequireCertificate ? ClientCertificateMode.RequireCertificate : ClientCertificateMode.AllowCertificate;
                httpsOptions.ClientCertificateValidation = validator.Validate;
                httpsOptions.CheckCertificateRevocation = (validator.RevocationCheck != X509RevocationMode.NoCheck);
            }
        });
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
