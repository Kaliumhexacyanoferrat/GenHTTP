using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Adapters.AspNetCore.Context;
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
using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Kestrel.Hosting;

/// <summary>
/// Drives Kestrel through the full ASP.NET Core generic host (<see cref="WebApplication"/>)
/// rather than the low-level <see cref="Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer"/>
/// API directly.
/// </summary>
/// <remarks>
/// An earlier draft used the raw <c>KestrelServer</c> constructor to avoid the generic host
/// altogether, but that path structurally cannot support HTTP/3: <c>KestrelServer</c>'s only
/// public constructor accepts nothing but a TCP <c>IConnectionListenerFactory</c>, and the real
/// HTTP/3 (QUIC) wiring lives in Kestrel's internal implementation, only reachable through the
/// generic host's own DI-based bootstrapping (<c>WebHostBuilderQuicExtensions.UseQuic</c>). Going
/// through <see cref="WebApplication"/> also removes the need to hand-construct a DI container
/// for Kestrel's internal <c>IHttpsConfigurationService</c>/<c>KestrelMetrics</c> - the host wires
/// all of that up itself.
/// </remarks>
internal sealed class KestrelServerBridge : IServer
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), BufferSize.Write);

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; private set; }

    public bool Development { get; }

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => Configuration.Logging;

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }

    private ServerConfiguration Configuration { get; }

    private WebApplication App { get; }

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

        App = Build();
    }

    #endregion

    #region Functionality

    private WebApplication Build()
    {
        // Passing no args of our own: this is meant to be embeddable in someone else's process,
        // and we don't want the generic host parsing *that* process's command line as if it were
        // ASP.NET Core options (e.g. a stray --urls flag colliding with our own endpoint config).
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });

        // Route Kestrel's own internal logging (and anything else DI-resolved) through the same
        // ILoggerFactory the rest of GenHTTP already uses, instead of the host's own default.
        builder.Services.AddSingleton(Configuration.Logging);

        if (Configuration.EndPoints.Any(e => e.EnableQuic))
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
        });

        var app = builder.Build();

        app.Run(HandleAsync);

        return app;
    }

    private static void Secure(ListenOptions options, EndPointConfiguration endpoint, SecurityConfiguration security)
    {
        options.Protocols = (endpoint.EnableQuic) ? HttpProtocols.Http1AndHttp2AndHttp3 : HttpProtocols.Http1AndHttp2;

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

        // The generic host logs its own "Now listening on: ..." lifecycle message, but with a
        // lowercase "listening" - other engines log "Listening on ..." (capital L, see e.g.
        // Internal's EndPoint.Start()), which is what TestServerLifecycleIsLogged actually checks
        // for, so keep logging this ourselves too rather than relying on the host's own message.
        var logger = Logging.CreateLogger<KestrelServerBridge>();

        foreach (var endpoint in EndPoints)
        {
            logger.LogInformation("Listening on {Address}:{Port} ({Settings})", endpoint.Address, endpoint.Port, endpoint.Secure ? "HTTPS" : "HTTP");
        }
    }

    private async Task HandleAsync(HttpContext context)
    {
        var clientContext = ContextPool.Get();

        try
        {
            clientContext.Apply(this, context.Features);

            try
            {
                var connectionFeature = context.Features.Get<IHttpConnectionFeature>();
                var tlsFeature = context.Features.Get<ITlsConnectionFeature>();
                var requestFeature = context.Features.GetRequiredFeature<IHttpRequestFeature>();

                var endPoint = ResolveEndPoint(connectionFeature, requestFeature);

                var request = clientContext.Request;

                request.Apply(this, endPoint, context.Features, connectionFeature?.RemoteIpAddress, tlsFeature?.ClientCertificate);

                // Captured before invoking the handler chain, as handlers may release the header
                // information by calling GetBody(HeaderAccess.Release) (mirrors ClientHandler.
                // HandleRequestAsync on the Internal engine).
                var headRequest = request.Header.Method == RequestMethod.Head;

                var response = await Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

                await clientContext.ResponseWriter.HandleAsync(request, response, headRequest);
            }
            catch (Exception e)
            {
                await SendErrorAsync(context, e);
            }
        }
        finally
        {
            ContextPool.Return(clientContext);
        }
    }

    /// <summary>
    /// Maps the connection this request arrived on back to the <see cref="IEndPoint"/> it
    /// was bound through, matched by local port (and, as a tiebreaker, by scheme) - the request
    /// feature set does not hand us the originating <see cref="IEndPoint"/> directly.
    /// </summary>
    private IEndPoint ResolveEndPoint(IHttpConnectionFeature? connection, IHttpRequestFeature requestFeature)
    {
        var port = connection?.LocalPort;
        var secure = string.Equals(requestFeature.Scheme, "https", StringComparison.OrdinalIgnoreCase);

        IEndPoint? portMatch = null;

        foreach (var candidate in EndPoints)
        {
            if (candidate.Port == port)
            {
                if (candidate.Secure == secure)
                {
                    return candidate;
                }

                portMatch ??= candidate;
            }
        }

        return portMatch ?? EndPoints[0];
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

            Logging.CreateLogger<KestrelServerBridge>().LogWarning(e, "Failed to handle client request");

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
