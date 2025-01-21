using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class EndPointCollection : List<IEndPoint>, IDisposable, IEndPointCollection
{

    #region Initialization

    public EndPointCollection(IServer server, IEnumerable<EndPointConfiguration> configuration, NetworkConfiguration networkConfiguration)
    {
        Server = server;
        NetworkConfiguration = networkConfiguration;

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
            return new InsecureEndPoint(Server, configuration.Address, configuration.Port, NetworkConfiguration);
        }

        return new SecureEndPoint(Server, configuration.Address, configuration.Port, configuration.Security, NetworkConfiguration);
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

    private NetworkConfiguration NetworkConfiguration { get; }

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            foreach (var endpoint in this)
            {
                try
                {
                    endpoint.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, null, e);
                }
            }

            Clear();

            _Disposed = true;
        }
    }

    #endregion

}
