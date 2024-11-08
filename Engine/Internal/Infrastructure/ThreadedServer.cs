using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal.Infrastructure.Endpoints;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal sealed class ThreadedServer : IServer
{
    private readonly EndPointCollection _EndPoints;

    #region Get-/Setters

    public string Version { get; }

    public bool Running => !_Disposed;

    public bool Development => Configuration.DevelopmentMode;

    public IHandler Handler { get; }

    public IServerCompanion? Companion { get; }

    public IEndPointCollection EndPoints => _EndPoints;

    internal ServerConfiguration Configuration { get; }

    #endregion

    #region Constructors

    internal ThreadedServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Companion = companion;
        Configuration = configuration;

        Handler = handler;

        _EndPoints = new EndPointCollection(this, configuration.EndPoints, configuration.Network);
    }

    private static async ValueTask PrepareHandlerAsync(IHandler handler, IServerCompanion? companion)
    {
        try
        {
            await handler.PrepareAsync();
        }
        catch (Exception e)
        {
            companion?.OnServerError(ServerErrorScope.General, null, e);
        }
    }

    #endregion

    #region Functionality

    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync(Handler, Companion);

        _EndPoints.Start();
    }

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    public ValueTask DisposeAsync()
    {
        if (!_Disposed)
        {
            _EndPoints.Dispose();

            _Disposed = true;
        }

        return new();
    }

    #endregion

}
