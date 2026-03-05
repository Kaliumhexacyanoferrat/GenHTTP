using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Internal.Parser;
using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;
using Microsoft.Extensions.ObjectPool;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Engine.Internal.Protocol;

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
    private static readonly StreamPipeReaderOptions ReaderOptions = new(MemoryPool<byte>.Shared, leaveOpen: true, bufferSize: 65536);

    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 65536);

    #region Get-/Setter

    internal IServer Server { get; }

    internal IEndPoint EndPoint { get; }

    internal NetworkConfiguration Configuration { get; }

    internal Socket Connection { get; }

    internal X509Certificate? ClientCertificate { get; set; }

    internal PoolBufferedStream Stream { get; }

    internal PipeWriter Writer { get; }

    private ResponseHandler ResponseHandler { get; }

    #endregion

    #region Initialization

    internal ClientHandler(Socket socket, PoolBufferedStream stream, X509Certificate? clientCertificate, IServer server, IEndPoint endPoint, NetworkConfiguration config)
    {
        Server = server;
        EndPoint = endPoint;

        Connection = socket;
        ClientCertificate = clientCertificate;

        Configuration = config;

        Stream = stream;
        Writer = PipeWriter.Create(stream);

        ResponseHandler = new ResponseHandler(Server, socket, Writer, Configuration);
    }

    #endregion

    #region Functionality

    internal async ValueTask Run()
    {
        try
        {
            await HandlePipe(PipeReader.Create(Stream, ReaderOptions)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
        }
        finally
        {
            try
            {
                await Stream.DisposeAsync();
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
            }

            try
            {
                Connection.Shutdown(SocketShutdown.Both);
                await Connection.DisconnectAsync(false);
                Connection.Close();

                Connection.Dispose();
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
            }
        }
    }

    private async ValueTask HandlePipe(PipeReader reader)
    {
        var context = ContextPool.Get();

        try
        {
            SequencePosition? consumed;

            while ((consumed = await RequestParser.TryParseAsync(reader, context.Request)) != null)
            {
                var status = await HandleRequest(context.Request, true); // todo: data remaining?

                if (status is Api.Protocol.Connection.Close)
                {
                    return;
                }

                reader.AdvanceTo(consumed.Value);
            }
        }
        catch (ProtocolException pe)
        {
            // client did something wrong
            await SendError(pe, 400);
            throw;
        }
        catch (Exception e)
        {
            // we did something wrong
            await SendError(e, 500);
            throw;
        }
        finally
        {
            ContextPool.Return(context);

            await reader.CompleteAsync();
        }
    }

    private async ValueTask<Connection> HandleRequest(Request request, bool dataRemaining)
    {
        // request.SetConnection(Server, EndPoint, Connection.GetAddress(), ClientCertificate);

        var keepAliveRequested = true; // request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? request.ProtocolType == HttpProtocol.Http11;

        var response = await Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested = false; // response.Connection is Api.Protocol.Connection.Close or Api.Protocol.Connection.Upgrade;

        var active = await ResponseHandler.Handle(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested);

        // flush if the client waits for this response
        // otherwise save flushes for improved performance when pipelining
        if (!dataRemaining && active)
        {
            await Stream.FlushAsync();
        }

        return (active && keepAliveRequested && !closeRequested) ? Api.Protocol.Connection.KeepAlive : Api.Protocol.Connection.Close;
    }

    private async ValueTask SendError(Exception e, int status)
    {
        try
        {
            var message = Server.Development ? e.ToString() : e.Message;

            // todo status code mapping


            var response = new ResponseBuilder().Raw()
                                                .Status(status, status == 500 ? "Internal Server Error"u8.ToArray() : "Bad Request"u8.ToArray())
                                                .Content(new StringContent(message))
                                                .Build();

            await ResponseHandler.Handle(null, response, HttpProtocol.Http10, false);
        }
        catch
        {
            /* no recovery here */
        }
    }

    #endregion

}
