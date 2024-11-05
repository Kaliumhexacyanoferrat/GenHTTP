using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;

using PooledAwait;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal abstract class EndPoint : IEndPoint
{

    #region Initialization

    protected EndPoint(IServer server, IPEndPoint endPoint, NetworkConfiguration configuration)
    {
        Server = server;

        Endpoint = endPoint;
        Configuration = configuration;

        IPAddress = endPoint.Address;
        Port = (ushort)endPoint.Port;

        try
        {
            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Socket.Bind(Endpoint);
            Socket.Listen(Configuration.Backlog);
        }
        catch (Exception e)
        {
            throw new BindingException($"Failed to bind to {endPoint}.", e);
        }

        Task = Task.Run(Listen);
    }

    #endregion

    #region Get-/Setters

    protected IServer Server { get; }

    protected NetworkConfiguration Configuration { get; }

    private Task Task { get; }

    private IPEndPoint Endpoint { get; }

    private Socket Socket { get; }

    #endregion

    #region Basic Information

    public IPAddress IPAddress { get; }

    public ushort Port { get; }

    public abstract bool Secure { get; }

    #endregion

    #region Functionality

    private async Task Listen()
    {
        try
        {
            do
            {
                Handle(await Socket.AcceptAsync());
            }
            while (!_ShuttingDown);
        }
        catch (Exception e)
        {
            if (!_ShuttingDown)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, null, e);
            }
        }
    }

    private void Handle(Socket client)
    {
        using var _ = ExecutionContext.SuppressFlow();

        Task.Run(() => Accept(client));
    }

    protected abstract PooledValueTask Accept(Socket client);

    protected PooledValueTask Handle(Socket client, Stream inputStream)
    {
        client.NoDelay = true;

        return new ClientHandler(client, inputStream, Server, this, Configuration).Run();
    }

    #endregion

    #region IDisposable Support

    private bool _Disposed, _ShuttingDown;

    protected virtual void Dispose(bool disposing)
    {
        _ShuttingDown = true;

        if (!_Disposed)
        {
            if (disposing)
            {
                try
                {
                    Socket.Close();
                    Socket.Dispose();

                    Task.Wait();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, null, e);
                }
            }

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

}
