using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Adapters.WiredIO;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

using Wired.IO.App;
using Wired.IO.Builder;
using Wired.IO.Http11;
using Wired.IO.Http11.Context;

namespace GenHTTP.Engine.WiredIO.Hosting;

internal sealed class WiredServer : IServer
{

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; private set; }

    public bool Development { get; }

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; }

    public IHandler Handler { get; }

    private ServerConfiguration Configuration { get; }

    private WiredApp<Http11Context> Application { get; }

    #endregion

    #region Initialization

    internal WiredServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler, Action<Builder<WiredHttp11, Http11Context>>? configurationHook, Action<WiredApp<Http11Context>>? applicationHook)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Companion = companion;
        Configuration = configuration;

        Development = configuration.DevelopmentMode;

        Handler = handler;

        var endpoints = new WiredEndpoints();

        endpoints.AddRange(configuration.EndPoints.Select(e => new WiredEndpoint(e.Address, e.Port, e.Security is not null)));

        EndPoints = endpoints;

        Application = Spawn(configurationHook, applicationHook);
    }

    #endregion

    #region Functionality

    private WiredApp<Http11Context> Spawn(Action<Builder<WiredHttp11, Http11Context>>? configurationHook, Action<WiredApp<Http11Context>>? applicationHook)
    {
        var builder = WiredApp.CreateBuilder();

        Configure(builder);

        configurationHook?.Invoke(builder);

        var app = builder.Build();

        app.BuildPipeline(Handler, this);

        applicationHook?.Invoke(app);

        return app;
    }

    public async ValueTask StartAsync()
    {
        await Handler.PrepareAsync();

        await Application.StartAsync();

        Running = true;
    }

    private void Configure(Builder<WiredHttp11, Http11Context> builder)
    {
        foreach (var endpoint in Configuration.EndPoints)
        {
            builder.Endpoint(endpoint.Address ?? IPAddress.Any, endpoint.Port);
        }

        var security = Secure(Configuration.EndPoints);

        if (security != null)
        {
            builder.UseTls(security);
        }
    }

    private static SslServerAuthenticationOptions? Secure(IEnumerable<EndPointConfiguration> endpoints)
    {
        foreach (var endpoint in endpoints)
        {
            // todo: wired does not allow security-by-endpoint
            if (endpoint.Security != null)
            {
                var security = endpoint.Security;
                var validator = security.CertificateValidator;

                return new SslServerAuthenticationOptions()
                {
                    EnabledSslProtocols = security.Protocols,
                    ServerCertificateSelectionCallback = (_, hostName) => security.CertificateProvider.Provide(hostName)!,
                    ClientCertificateRequired = validator?.RequireCertificate ?? false,
                    CertificateRevocationCheckMode = validator?.RevocationCheck ?? X509RevocationMode.NoCheck,
                    RemoteCertificateValidationCallback = (_, certificate, chain, sslPolicyErrors) => validator == null || validator.Validate(certificate, chain, sslPolicyErrors)
                };
            }
        }

        return null;
    }

    #endregion

    #region Lifecycle

    private bool _Disposed;

    public async ValueTask DisposeAsync()
    {
        if (!_Disposed)
        {
            await Application.StopAsync();

            Application.Dispose();

            _Disposed = true;
        }
    }

    #endregion

}
