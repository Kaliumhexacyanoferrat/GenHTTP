using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal abstract class EndPoint : IEndPoint, IDisposable
    {

        #region Get-/Setters

        protected IServer Server { get; }

        protected NetworkConfiguration Configuration { get; }

        private Thread Thread { get; set; }
        
        private IPEndPoint Endpoint { get; }
        
        private Socket Socket { get; }

        #endregion

        #region Basic Information

        public IPAddress IPAddress => Endpoint.Address;

        public ushort Port => (ushort)Endpoint.Port;

        public abstract bool Secure { get; }

        #endregion

        #region Initialization

        protected EndPoint(IServer server, IPEndPoint endPoint, NetworkConfiguration configuration)
        {
            Server = server;

            Endpoint = endPoint;
            Configuration = configuration;

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
            

            Thread = new Thread(new ThreadStart(async() => await Listen()));
            Thread.Start();
        }

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
            if (!ThreadPool.QueueUserWorkItem(new WaitCallback(async (_) => await Accept(client))))
            {
                var error = new NetworkException("Thread pool did not accept worker. Connection dropped.");
                Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, error);

                try
                {
                    client.Disconnect(false);
                    client.Close();

                    client.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        protected abstract Task Accept(Socket client);

        protected async Task Handle(Socket client, Stream inputStream)
        {
            inputStream.ReadTimeout = (int)Configuration.RequestReadTimeout.TotalMilliseconds;
            await new ClientHandler(client, inputStream, Server, this, Configuration).Run();
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

                        Thread.Join();
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
        }

        #endregion

    }

}
