using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Internal.Infrastructure.Endpoints;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal sealed class ThreadedServer : IServer
{
    private readonly EndPointCollection _endPoints;

    private readonly PropertyBag _properties = new();

    #region Get-/Setters

    public string Version { get; }

    public bool Running => !_disposed;

    public bool Development => Configuration.DevelopmentMode;

    public IHandler Handler { get; }

    public IPropertyBag Properties => _properties;

    public IEndPointCollection EndPoints => _endPoints;

    internal ServerConfiguration Configuration { get; }

    #endregion

    #region Constructors

    internal ThreadedServer(ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Configuration = configuration;

        Handler = handler;

        _endPoints = new EndPointCollection(this, configuration.EndPoints);
    }

    #endregion

    #region Functionality

    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync(Handler);

        _endPoints.Start();
    }

    private async ValueTask PrepareHandlerAsync(IHandler handler)
    {
        try
        {
            await handler.PrepareAsync(this);
        }
        catch
        {
            // todo: logging
        }
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _endPoints.Dispose();

            _disposed = true;
        }

        return new();
    }

    #endregion

}
