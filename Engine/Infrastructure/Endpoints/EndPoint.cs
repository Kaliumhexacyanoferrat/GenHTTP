using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Infrastructure.Endpoints
{

    internal abstract class EndPoint : IEndPoint
    {

        #region Get-/Setters

        protected IServer Server { get; }

        protected NetworkConfiguration Configuration { get; }

        private Task Task { get; set; }

        private IPEndPoint Endpoint { get; }

        private Socket Socket { get; }

        #endregion

        #region Basic Information

        public IPAddress IPAddress { get; }

        public ushort Port { get; }

        public abstract bool Secure { get; }

        #endregion

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
                Socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                Socket.Bind(Endpoint);
                Socket.Listen(Configuration.Backlog);
            }
            catch (Exception e)
            {
                throw new BindingException($"Failed to bind to {endPoint}.", e);
            }

            Task = Task.Run(() => Listen().ConfigureAwait(false));
        }

        #endregion

        #region Functionality

        private async Task Listen()
        {
            try
            {
                do
                {
                    Handle(await Socket.AcceptAsync().ConfigureAwait(false));
                }
                while (!shuttingDown);
            }
            catch (Exception e)
            {
                if (!shuttingDown)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
                }
            }
        }

        private void Handle(Socket client)
        {
            using var _ = ExecutionContext.SuppressFlow();

            Task.Run(() => Accept(client).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        protected abstract ValueTask Accept(Socket client);

        protected ValueTask Handle(Socket client, Stream inputStream)
        {
            return new ClientHandler(client, inputStream, Server, this, Configuration).Run();
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false, shuttingDown = false;

        protected virtual void Dispose(bool disposing)
        {
            shuttingDown = true;

            if (!disposed)
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
                        Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
                    }
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
