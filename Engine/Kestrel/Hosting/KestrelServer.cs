using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Kestrel.Types;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

internal sealed class KestrelServer : IServer
{

    #region Get-/Setters

    public string Version { get; }

    public bool Running { get; }

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

        Handler = handler;

        Application = Spawn();
    }

    #endregion

    #region Functionality

    private WebApplication Spawn()
    {
        var builder = WebApplication.CreateBuilder();

        Configure(builder);

        var app = builder.Build();

        app.Run(MapAsync);

        return app;
    }

    private void Configure(WebApplicationBuilder builder)
    {

    }

    private async Task MapAsync(HttpContext context)
    {
        using var request = new Request(context);

        // todo: Handler.PrepareAsync()

        using var response = await Handler.HandleAsync(request);

        if (response == null)
        {
            context.Response.StatusCode = 204;
        }
        else
        {
            Write(response, context);
        }
    }

    private void Write(IResponse response, HttpContext context)
    {

    }

    #endregion

    #region Lifecycle

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            Task.Run(() => Application.DisposeAsync());

            _Disposed = true;
        }
    }

    #endregion

}
