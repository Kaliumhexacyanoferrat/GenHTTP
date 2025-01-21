using System.Reflection;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Adapters.AspNetCore;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Kestrel.Hosting;

internal sealed class KestrelServer : IServer
{

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; private set; }

    public bool Development { get; }

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; }

    public IHandler Handler { get; }

    private ServerConfiguration Configuration { get; }

    private WebApplication Application { get; }

    #endregion

    #region Initialization

    internal KestrelServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler, Action<WebApplicationBuilder>? configurationHook, Action<WebApplication>? applicationHook)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Companion = companion;
        Configuration = configuration;

        Development = configuration.DevelopmentMode;

        Handler = handler;

        var endpoints = new KestrelEndpoints();

        endpoints.AddRange(configuration.EndPoints.Select(e => new KestrelEndpoint(e.Address, e.Port, e.Security is not null)));

        EndPoints = endpoints;

        Application = Spawn(configurationHook, applicationHook);
    }

    #endregion

    #region Functionality

    private WebApplication Spawn(Action<WebApplicationBuilder>? configurationHook, Action<WebApplication>? applicationHook)
    {
        var builder = WebApplication.CreateBuilder();

        Configure(builder);

        configurationHook?.Invoke(builder);

        var app = builder.Build();

        app.Run(Handler, server: this);

        applicationHook?.Invoke(app);

        return app;
    }

    public async ValueTask StartAsync()
    {
        await Handler.PrepareAsync();

        await Application.StartAsync();

        Running = true;
    }

    private void Configure(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AllowSynchronousIO = true;

            options.Limits.MaxRequestBodySize = null;

            foreach (var endpoint in Configuration.EndPoints)
            {
                if (endpoint.Address != null)
                {
                    if (endpoint.Security != null)
                    {
                        options.Listen(endpoint.Address, endpoint.Port, listenOptions => Secure(listenOptions, endpoint, endpoint.Security));
                    }
                    else
                    {
                        options.Listen(endpoint.Address, endpoint.Port);
                    }
                }
                else
                {
                    if (endpoint.Security != null)
                    {
                        options.ListenAnyIP(endpoint.Port, listenOptions => Secure(listenOptions, endpoint, endpoint.Security));
                    }
                    else
                    {
                        options.ListenAnyIP(endpoint.Port);
                    }
                }
            }
        });
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

    private bool _Disposed;

    public async ValueTask DisposeAsync()
    {
        if (!_Disposed)
        {
            await Application.StopAsync();

            await Application.DisposeAsync();

            _Disposed = true;
        }
    }

    #endregion

}
