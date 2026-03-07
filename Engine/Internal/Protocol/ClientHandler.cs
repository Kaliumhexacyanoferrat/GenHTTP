using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Shared.Types;

using Glyph11;
using Glyph11.Parser.FlexibleParser;
using Glyph11.Protocol;

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
internal sealed class ClientHandler(ClientContext context)
{
    private static readonly TimeSpan InitialReadTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(60);

    private CancellationTokenSource _cts = new();
    
    #region Functionality

    internal async ValueTask Run()
    {
        var connection = context.Connection;
        
        try
        {
            await HandlePipe(context.Reader).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            context.Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, connection.GetAddress(), e);
        }
        finally
        {
            try
            {
                await context.Stream.DisposeAsync();
            }
            catch (Exception e)
            {
                context.Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, connection.GetAddress(), e);
            }

            try
            {
                connection.Shutdown(SocketShutdown.Both);
                
                await connection.DisconnectAsync(false);
                
                connection.Close();

                connection.Dispose();
            }
            catch (Exception e)
            {
                context.Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, connection.GetAddress(), e);
            }
        }
    }

    private async ValueTask HandlePipe(PipeReader reader)
    {
        ResetCts(InitialReadTimeout);
        
        var request = context.Request;
        var into = request.Source;

        try
        {
            while (true)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var buffer = result.Buffer;

                var handledRequest = false;

                while (TryParseRequest(ref buffer, into))
                {
                    request.Apply();

                    var status = await HandleRequest(request);

                    if (status is Connection.Close)
                    {
                        return;
                    }

                    request.Reset();
                    
                    handledRequest = true;
                }
                
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (!handledRequest) break;
                
                await context.Writer.FlushAsync();
                
                if (result.IsCompleted) break;

                ResetCts(KeepAliveTimeout);
            }
        }
        catch (HttpParseException pe)
        {
            // client did something wrong
            SendError(pe, 400);
            throw;
        }
        catch (Exception e)
        {
            // we did something wrong
            SendError(e, 500);
            throw;
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!FlexibleParser.TryExtractFullHeader(ref buffer, into, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead);
        return true;
    }

    private async ValueTask<Connection> HandleRequest(Request request)
    {
        // request.SetConnection(Server, EndPoint, Connection.GetAddress(), ClientCertificate);

        var
            keepAliveRequested =
                true; // request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? request.ProtocolType == HttpProtocol.Http11;

        var response = await context.Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested = false; // response.Connection is Api.Protocol.Connection.Close or Api.Protocol.Connection.Upgrade;

        var active = context.ResponseHandler.Handle(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested);
        
        return (active && keepAliveRequested && !closeRequested) ? Connection.KeepAlive : Connection.Close;
    }

    private void SendError(Exception e, int status)
    {
        try
        {
            var message = context.Server.Development ? e.ToString() : e.Message;

            // todo status code mapping

            var response = new ResponseBuilder().Raw()
                .Status(status, status == 500 ? "Internal Server Error"u8.ToArray() : "Bad Request"u8.ToArray())
                .Content(new StringContent(message))
                .Build();

            context.ResponseHandler.Handle(null, response, HttpProtocol.Http10, false);
        }
        catch
        {
            /* no recovery here */
        }
    }

    private void ResetCts(TimeSpan timeout)
    {
        if (!_cts.TryReset())
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        _cts.CancelAfter(timeout);
    }

    #endregion
    
}