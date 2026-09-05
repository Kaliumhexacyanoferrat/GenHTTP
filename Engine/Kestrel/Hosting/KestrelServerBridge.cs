using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Adapters.AspNetCore.Mapping;

using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using GenHttpProtocols = GenHTTP.Api.Infrastructure.HttpProtocols;
using KestrelProtocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols;

namespace GenHTTP.Engine.Kestrel.Hosting;

internal sealed class KestrelServerBridge : IServer
{
    private readonly KestrelEndpoints _endpoints = new();

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; private set; }

    public bool Development { get; }

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => Configuration.Logging;

    public IEndPointCollection EndPoints => _endpoints;

    public IHandler Handler { get; }

    private ServerConfiguration Configuration { get; }

    private WebApplication App { get; }

    #endregion

    #region Initialization

    internal KestrelServerBridge(ServerConfiguration configuration, IHandler handler, Action<WebApplicationBuilder>? configHook, Action<WebApplication>? appHook)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Configuration = configuration;

        Development = configuration.DevelopmentMode;

        Handler = handler;

        App = Build(configHook, appHook);
    }

    #endregion

    #region Functionality

    private WebApplication Build(Action<WebApplicationBuilder>? configHook, Action<WebApplication>? appHook)
    {
        // No args of our own: avoids the generic host parsing the embedding process's command
        // line as ASP.NET Core options (e.g. a stray --urls flag).
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });

        // Route Kestrel's own internal logging through the same ILoggerFactory GenHTTP uses.
        builder.Services.AddSingleton(Configuration.Logging);

        if (Configuration.EndPoints.Any(e => e.Protocols.HasFlag(Api.Infrastructure.HttpProtocols.Http3)))
        {
            builder.WebHost.UseQuic();
        }

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AllowSynchronousIO = true;

            options.Limits.MaxRequestBodySize = null;

            // ResponseWriter sets its own "Server: GenHTTP/x.y" header - don't let
            // Kestrel add its own "Server: Kestrel" alongside it.
            options.AddServerHeader = false;

            foreach (var endpoint in Configuration.EndPoints)
            {
                var validatedProtocols = Protocols.Validate(endpoint);

                if ((endpoint.Address == null) || (endpoint.DualStack && (endpoint.Address.Equals(IPAddress.Any) || endpoint.Address.Equals(IPAddress.IPv6Any))))
                {
                    options.ListenAnyIP(endpoint.Port, listenOptions => Configure(listenOptions, endpoint, validatedProtocols));
                }
                else
                {
                    options.Listen(endpoint.Address, endpoint.Port, listenOptions => Configure(listenOptions, endpoint, validatedProtocols));
                }

                _endpoints.Add(new KestrelEndpoint(endpoint.Address, endpoint.Port, validatedProtocols, endpoint.DualStack, endpoint.Security is not null));
            }
        });

        configHook?.Invoke(builder);

        var app = builder.Build();

        appHook?.Invoke(app);

        app.Run(HandleAsync);

        return app;
    }

    private static void Configure(ListenOptions options, EndPointConfiguration endpoint, GenHttpProtocols validatedProtocols)
    {
        options.Protocols = MapProtocol(validatedProtocols);

        if (endpoint.Security is not null)
        {
            Secure(options, endpoint.Security);
        }
    }

    private static void Secure(ListenOptions options, SecurityConfiguration security)
    {
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

    public async ValueTask StartAsync()
    {
        await Handler.PrepareAsync(this);

        await App.StartAsync();

        Running = true;

        var logger = Logging.CreateLogger<KestrelServerBridge>();

        foreach (var endpoint in EndPoints)
        {
            logger.LogInformation("Listening on {Address}:{Port} ({Settings})", endpoint.Address, endpoint.Port, endpoint.Secure ? "HTTPS" : "HTTP");
        }
    }

    private async Task HandleAsync(HttpContext context)
    {
        try
        {
            await Bridge.MapAsync(context, Handler, this, requireResponse: true);
        }
        catch (Exception e)
        {
            await SendErrorAsync(context, e);
        }
    }

    private async Task SendErrorAsync(HttpContext context, Exception e)
    {
        try
        {
            var responseFeature = context.Features.GetRequiredFeature<IHttpResponseFeature>();

            if (responseFeature.HasStarted)
            {
                // headers (or worse, body bytes) are already on the wire - nothing sane to do
                return;
            }

            if (!ConnectionExceptions.IsGracefulDisconnect(e))
            {
                Logging.CreateLogger<KestrelServerBridge>().LogWarning(e, "Failed to handle client request");
            }

            var message = Development ? e.ToString() : "Internal Server Error";
            var body = Encoding.UTF8.GetBytes(message);

            responseFeature.StatusCode = (int)ResponseStatus.InternalServerError;
            responseFeature.Headers.ContentType = "text/plain";
            responseFeature.Headers.ContentLength = body.Length;

            var bodyFeature = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

            await bodyFeature.Stream.WriteAsync(body);
        }
        catch
        {
            /* no recovery here */
        }
    }

    private static KestrelProtocols MapProtocol(GenHttpProtocols requested)
    {
        var protocols = KestrelProtocols.None;

        if (requested.HasFlag(GenHttpProtocols.Http1))
        {
            protocols |= KestrelProtocols.Http1;
        }

        if (requested.HasFlag(GenHttpProtocols.Http2))
        {
            protocols |= KestrelProtocols.Http2;
        }

        if (requested.HasFlag(GenHttpProtocols.Http3))
        {
            protocols |= KestrelProtocols.Http3;
        }

        return protocols;
    }

    #endregion

    #region Lifecycle

    private bool _disposed;

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await App.StopAsync();

            await App.DisposeAsync();

            _disposed = true;
        }
    }

    #endregion

}
