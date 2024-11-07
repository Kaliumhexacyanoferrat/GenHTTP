using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Kestrel.Types;
using GenHTTP.Engine.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

    internal KestrelServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Companion = companion;
        Configuration = configuration;

        Development = configuration.DevelopmentMode;

        Handler = handler;

        var endpoints = new KestrelEndpoints();

        endpoints.AddRange(configuration.EndPoints.Select(e => new KestrelEndpoint(e.Address, e.Port, e.Security is not null)));

        EndPoints = endpoints;

        Application = Spawn();

        using var startEvent = new ManualResetEventSlim(false);

        Task.Run(async () =>
        {
            await Start();
            startEvent.Set();
        });

        startEvent.Wait();
    }

    #endregion

    #region Functionality

    private WebApplication Spawn()
    {
        var builder = WebApplication.CreateBuilder();

        Configure(builder);

        var app = builder.Build();

        app.Run(async (context) => await MapAsync(context));

        return app;
    }

    private async ValueTask Start()
    {
        await Handler.PrepareAsync();

        await Application.StartAsync();

        Running = true;
    }

    private void Configure(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            foreach (var endpoint in Configuration.EndPoints)
            {
                if (endpoint.Security != null)
                {
                    options.Listen(endpoint.Address, endpoint.Port, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            httpsOptions.SslProtocols = endpoint.Security.Protocols;
                            httpsOptions.ServerCertificateSelector = (_, hostName) => endpoint.Security.Certificate.Provide(hostName);
                        });
                    });
                }
                else
                {
                    options.Listen(endpoint.Address, endpoint.Port);
                }
            }
        });
    }

    private async ValueTask MapAsync(HttpContext context)
    {
        try
        {
            using var request = new Request(this, context);

            using var response = await Handler.HandleAsync(request);

            if (response == null)
            {
                context.Response.StatusCode = 204;
            }
            else
            {
                await WriteAsync(response, context);

                Companion?.OnRequestHandled(request, response);
            }
        }
        catch (Exception e)
        {
            Companion?.OnServerError(ServerErrorScope.ServerConnection, context.Connection.RemoteIpAddress, e);
            throw;
        }
    }

    private async ValueTask WriteAsync(IResponse response, HttpContext context)
    {
        var target = context.Response;

        target.StatusCode = response.Status.RawStatus;

        foreach (var header in response.Headers)
        {
            target.Headers[header.Key] = header.Value;
        }

        if (response.Modified != null)
        {
            target.Headers.LastModified = response.Modified.Value.ToUniversalTime().ToString("r");
        }

        if (response.Expires != null)
        {
            target.Headers.Expires = response.Expires.Value.ToUniversalTime().ToString("r");
        }

        if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                if (cookie.Value.MaxAge != null)
                {
                    target.Cookies.Append(cookie.Key, cookie.Value.Value, new() { MaxAge = TimeSpan.FromSeconds(cookie.Value.MaxAge.Value)});
                }
                else
                {
                    target.Cookies.Append(cookie.Key, cookie.Value.Value);
                }
            }
        }

        if (response.Content != null)
        {
            target.ContentLength = (long?)response.ContentLength ?? (long?)response.Content.Length;

            target.ContentType = response.ContentType?.Charset != null ? $"{response.ContentType?.RawType}; charset={response.ContentType?.Charset}" : response.ContentType?.RawType;

            if (response.ContentEncoding != null)
            {
                target.Headers.ContentEncoding = response.ContentEncoding;
            }

            await response.Content.WriteAsync(target.Body, 4096); // todo
        }
    }

    #endregion

    #region Lifecycle

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            Task.Run(async () =>
            {
                await Application.StopAsync();

                await Application.DisposeAsync();
            });

            _Disposed = true;
        }
    }

    #endregion

}
