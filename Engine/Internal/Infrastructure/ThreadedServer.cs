using System.Diagnostics;
using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Internal.Infrastructure.Endpoints;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal sealed class ThreadedServer : IServer
{
    private readonly EndPointCollection _endPoints;

    private readonly PropertyBag _properties = new();

    private readonly ILogger _logger;

    #region Get-/Setters

    public string Version { get; }

    public bool Running => !_disposed;

    public bool Development => Configuration.DevelopmentMode;

    public IHandler Handler { get; }

    public IPropertyBag Properties => _properties;

    public ILoggerFactory Logging => Configuration.Logging;

    public IEndPointCollection EndPoints => _endPoints;

    internal ServerConfiguration Configuration { get; }

    #endregion

    #region Constructors

    internal ThreadedServer(ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Configuration = configuration;

        Handler = handler;

        _logger = configuration.Logging.CreateLogger<ThreadedServer>();

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
            var start = Stopwatch.GetTimestamp();
            
            await handler.PrepareAsync(this);

            var elapsed = Stopwatch.GetElapsedTime(start);
            
            _logger.LogInformation("Prepared handlers in {ElapsedMs:0.##} ms", elapsed.TotalMilliseconds);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to prepare the handler chain");
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
