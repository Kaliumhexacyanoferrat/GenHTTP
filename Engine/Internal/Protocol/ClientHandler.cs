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

    private static readonly ReadOnlyMemory<byte> ConnectionHeader = "Connection"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> KeepAliveValue = "Keep-Alive"u8.ToArray();

    private CancellationTokenSource _cts = new();

    #region Functionality

    internal async ValueTask RunAsync()
    {
        var connection = context.Connection;

        try
        {
            await HandlePipeAsync(context.Reader).ConfigureAwait(false);
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

    private async ValueTask HandlePipeAsync(PipeReader reader)
    {
        ResetCts(InitialReadTimeout);

        var request = context.Request;
        var into = request.Source;

        ReadResult readResult = default;
        var dataRemaining = false;

        try
        {
            while (context.Server.Running)
            {
                if (!dataRemaining)
                {
                    try
                    {
                        readResult = await reader.ReadAsync(_cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (IOException e) when (e.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionReset or SocketError.ConnectionAborted })
                    {
                        return;
                    }
                    catch (IOException e) when (e.Message.Contains("Broken pipe", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                dataRemaining = false;

                var buffer = readResult.Buffer;

                if (!TryParseRequest(ref buffer, into))
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    break;
                }

                request.Apply(context.Server, context.EndPoint, reader, buffer.Start);

                var status = await HandleRequestAsync(request);

                if (status is Connection.Close)
                {
                    await context.Writer.FlushAsync();
                    return;
                }

                await request.DrainAsync();

                request.Reset();

                if (readResult.IsCompleted) break;

                if (reader.TryRead(out var next))
                {
                    readResult = next;
                    dataRemaining = true;
                }
                else
                {
                    await context.Writer.FlushAsync();
                    ResetCts(KeepAliveTimeout);
                }
            }
        }
        catch (HttpParseException pe)
        {
            // client did something wrong
            await SendErrorAsync(pe, ResponseStatus.BadRequest);
            throw;
        }
        catch (Exception e)
        {
            // we did something wrong
            await SendErrorAsync(e, ResponseStatus.InternalServerError);
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

    private async ValueTask<Connection> HandleRequestAsync(Request request)
    {
        // request.SetConnection(Server, EndPoint, Connection.GetAddress(), ClientCertificate);

        var header = request.Header;

        var connectionHeader = header.Headers.GetEntry(ConnectionHeader);

        var keepAliveRequested = connectionHeader?.Span.SequenceEqual(KeepAliveValue.Span) ?? (header.Protocol == HttpProtocol.Http11);

        var response = await context.Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested = response.Mode is Connection.Close or Connection.Upgrade;

        var active = await context.ResponseHandler.HandleAsync(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested);

        return (active && keepAliveRequested && !closeRequested) ? Connection.KeepAlive : Connection.Close;
    }

    private ValueTask<bool> SendErrorAsync(Exception e, ResponseStatus status)
    {
        try
        {
            var message = context.Server.Development ? e.ToString() : e.Message;

            // todo status code mapping

            var response = new ResponseBuilder()
                           .Status(status)
                           .Content(new StringContent(message))
                           .Build();

            return context.ResponseHandler.HandleAsync(null, response, HttpProtocol.Http10, false);
        }
        catch
        {
            /* no recovery here */
            return ValueTask.FromResult(false);
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
