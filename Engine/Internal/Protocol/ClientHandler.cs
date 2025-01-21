using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal.Protocol.Parser;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

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

    #region Get-/Setter

    internal IServer Server { get; }

    internal IEndPoint EndPoint { get; }

    internal NetworkConfiguration Configuration { get; }

    internal Socket Connection { get; }

    internal X509Certificate? ClientCertificate { get; set; }

    internal Stream Stream { get; }

    private bool? KeepAlive { get; set; }

    private ResponseHandler ResponseHandler { get; }

    #endregion

    #region Initialization

    internal ClientHandler(Socket socket, Stream stream, X509Certificate? clientCertificate, IServer server, IEndPoint endPoint, NetworkConfiguration config)
    {
        Server = server;
        EndPoint = endPoint;

        Connection = socket;
        ClientCertificate = clientCertificate;

        Configuration = config;

        Stream = stream;

        ResponseHandler = new ResponseHandler(Server, socket, Stream, Configuration);
    }

    #endregion

    #region Functionality

    internal async ValueTask Run()
    {
        var status = ConnectionStatus.Close;

        try
        {
            status = await HandlePipe(PipeReader.Create(Stream, ReaderOptions)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
        }
        finally
        {
            if (status != ConnectionStatus.Upgraded)
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
    }

    private async ValueTask<ConnectionStatus> HandlePipe(PipeReader reader)
    {
        try
        {
            using var buffer = new RequestBuffer(reader, Configuration);

            var parser = new RequestParser(Configuration);

            RequestBuilder? request;

            try
            {
                while (Server.Running && (request = await parser.TryParseAsync(buffer)) is not null)
                {
                    var status = await HandleRequest(request, !buffer.ReadRequired);

                    if (status is ConnectionStatus.Close or ConnectionStatus.Upgraded)
                    {
                        return status;
                    }
                }
            }
            catch (ProtocolException pe)
            {
                // client did something wrong
                await SendError(pe, ResponseStatus.BadRequest);
                throw;
            }
            catch (Exception e)
            {
                // we did something wrong
                await SendError(e, ResponseStatus.InternalServerError);
                throw;
            }
        }
        finally
        {
            await reader.CompleteAsync();
        }

        return ConnectionStatus.Close;
    }

    private async ValueTask<ConnectionStatus> HandleRequest(RequestBuilder builder, bool dataRemaining)
    {
        using var request = builder.Connection(Server, Connection, Stream, EndPoint, Connection.GetAddress(), ClientCertificate).Build();

        KeepAlive ??= request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? request.ProtocolType == HttpProtocol.Http11;

        var keepAlive = KeepAlive.Value;

        using var response = await Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        if (response.Upgraded)
        {
            return ConnectionStatus.Upgraded;
        }

        var active = await ResponseHandler.Handle(request, response, keepAlive, dataRemaining);

        return (active && keepAlive) ? ConnectionStatus.KeepAlive : ConnectionStatus.Close;
    }

    private async ValueTask SendError(Exception e, ResponseStatus status)
    {
        try
        {
            var message = Server.Development ? e.ToString() : e.Message;

            var response = new ResponseBuilder().Status(status)
                                                .Content(new StringContent(message))
                                                .Build();

            await ResponseHandler.Handle(null, response, false, false);
        }
        catch
        {
            /* no recovery here */
        }
    }

    #endregion

}
