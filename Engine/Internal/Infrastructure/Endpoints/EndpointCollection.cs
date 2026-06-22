using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class EndPointCollection : List<IEndPoint>, IDisposable, IEndPointCollection
{

    #region Initialization

    public EndPointCollection(IServer server, IEnumerable<EndPointConfiguration> configuration)
    {
        Server = server;
        Logger = server.Logging.CreateLogger<EndPointCollection>();

        foreach (var config in configuration)
        {
            Add(Build(config));
        }
    }

    #endregion

    #region Functionality

    private EndPoint Build(EndPointConfiguration configuration)
    {
        if (configuration.Security is null)
        {
            return new InsecureEndPoint(Server, configuration.Address, configuration.Port, configuration.DualStack);
        }

        return new SecureEndPoint(Server, configuration.Address, configuration.Port, configuration.DualStack, configuration.Security);
    }

    internal void Start()
    {
        foreach (var endPoint in this)
        {
            (endPoint as EndPoint)?.Start();
        }
    }

    #endregion

    #region Get-/Setters

    private IServer Server { get; }

    private ILogger Logger { get; }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var endpoint in this)
            {
                try
                {
                    endpoint.Dispose();
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Failed to dispose endpoint");
                }
            }

            Clear();

            _disposed = true;
        }
    }

    #endregion

}
