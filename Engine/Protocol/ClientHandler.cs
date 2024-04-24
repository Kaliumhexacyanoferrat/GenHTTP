using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Infrastructure.Configuration;
using GenHTTP.Engine.Infrastructure.Transport;
using GenHTTP.Engine.Protocol;
using GenHTTP.Engine.Protocol.Parser;

using PooledAwait;

namespace GenHTTP.Engine
{

    /// <summary>
    /// Maintains a single connection to a client, continuously reading
    /// requests and generating responses.
    /// </summary>
    /// <remarks>
    /// Implements keep alive and maintains the connection state (e.g. by
    /// closing it after the last request has been handled).
    /// </remarks>
    internal sealed class ClientHandler
    {

        #region Get-/Setter

        public IServer Server { get; }

        public IEndPoint EndPoint { get; }

        internal NetworkConfiguration Configuration { get; }

        internal SocketConnection Connection { get; }

        internal Stream Stream { get; }

        private bool? KeepAlive { get; set; }

        private ResponseHandler ResponseHandler { get; set; }

        #endregion

        #region Initialization

        internal ClientHandler(SocketConnection socket, IServer server, IEndPoint endPoint, NetworkConfiguration config)
        {
            Server = server;
            EndPoint = endPoint;

            Configuration = config;
            Connection = socket;

            Stream = socket.Pipe.Transport.Output.AsStream();

            ResponseHandler = new ResponseHandler(Server, Stream, Configuration); 
        }

        #endregion

        #region Functionality

        internal async PooledValueTask Run()
        {
            try
            {
                await HandlePipe(Connection.Pipe.Transport.Input).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
            finally
            {
                try
                {
                    await Stream.DisposeAsync();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private async PooledValueTask HandlePipe(PipeReader reader)
        {
            try
            {
                using var buffer = new RequestBuffer(reader, Configuration);

                var parser = new RequestParser(Configuration);

                RequestBuilder? request;

                while (Server.Running && (request = await parser.TryParseAsync(buffer)) is not null)
                {
                    if (!await HandleRequest(request, !buffer.ReadRequired))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // todo
                var m = e.Message;
            }
            finally
            {
                await reader.CompleteAsync();
            }
        }

        private async PooledValueTask<bool> HandleRequest(RequestBuilder builder, bool dataRemaining)
        {
            var address = (Connection.Socket.RemoteEndPoint as IPEndPoint)?.Address;

            using var request = builder.Connection(Server, EndPoint, address).Build();
            
            KeepAlive ??= request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? true;

            bool keepAlive = (bool)KeepAlive;

            using var response = await Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");
            
            var success = await ResponseHandler.Handle(request, response, keepAlive, dataRemaining);

            if (!success || !keepAlive)
            {
                return false;
            }

            return true;
        }

        #endregion

    }

}
