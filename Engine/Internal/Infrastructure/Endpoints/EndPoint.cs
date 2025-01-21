﻿using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;
using PooledAwait;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal abstract class EndPoint : IEndPoint
{

    #region Get-/Setters

    protected IServer Server { get; }

    protected NetworkConfiguration Configuration { get; }

    private Task? Task { get; set; }

    private Socket? Socket { get; set; }

    public IReadOnlyList<IPAddress>? Addresses { get; }

    public ushort Port { get; }

    public abstract bool Secure { get; }

    #endregion

    #region Initialization

    protected EndPoint(IServer server, IReadOnlyList<IPAddress>? addresses, ushort port, NetworkConfiguration configuration)
    {
        Server = server;

        Configuration = configuration;

        Addresses = addresses;
        Port = port;
    }

    #endregion

    #region Functionality

    public void Start()
    {
        var bindings = Addresses ?? [ IPAddress.IPv6Any ];

        try
        {
            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            foreach (var address in bindings)
            {
                Socket.Bind(new IPEndPoint(address, Port));
            }

            Socket.Listen(Configuration.Backlog);
        }
        catch (Exception e)
        {
            throw new BindingException($"Failed to bind to on [ {string.Join(", ", bindings)} ] port {Port}.", e);
        }

        Task = Task.Run(Listen);
    }

    private async Task Listen()
    {
        if (Socket == null) throw new InvalidOperationException("The endpoint has not been started");

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

    protected PooledValueTask Handle(Socket client, Stream inputStream, X509Certificate? clientCertificate = null)
    {
        client.NoDelay = true;

        return new ClientHandler(client, inputStream, clientCertificate, Server, this, Configuration).Run();
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
                    if (Socket != null && Task != null)
                    {
                        Socket.Close();
                        Socket.Dispose();

                        Task.Wait();
                    }
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
