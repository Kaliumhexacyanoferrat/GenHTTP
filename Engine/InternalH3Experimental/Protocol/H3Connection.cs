using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using System.Threading.Channels;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Glyph3;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// One HTTP/3 connection: MsQuic underneath, Glyph3 on top, GenHTTP's handler chain in the middle.
/// </summary>
/// <remarks>
/// Glyph3 is a single state machine, so every call into it is funnelled through one channel and one
/// consumer - the pump. Handlers start on that thread and, if they suspend, resume back onto it
/// through PumpContext, so several requests can be in flight without the parser seeing two threads.
/// </remarks>
internal sealed class H3Connection : IHttp3Transport, IAsyncDisposable
{
    private readonly QuicConnection _quic;

    private readonly IServer _server;

    private readonly IEndPoint _endPoint;

    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<long, QuicStream> _streams = new();

    private readonly Queue<QuicStream> _spareUniStreams = new();

    private readonly Channel<Inbound> _ingress =
        Channel.CreateUnbounded<Inbound>(new UnboundedChannelOptions { SingleReader = true });

    private readonly Channel<Outbound> _egress =
        Channel.CreateUnbounded<Outbound>(new UnboundedChannelOptions { SingleReader = true });

    private Http3Connection? _h3;

    private const int ServerUniStreams = 1;

    // Stream bytes, a stream ending, or a continuation that must run on the pump thread.
    private readonly record struct Inbound(long StreamId, byte[]? Buffer, int Length, bool Fin, bool Closed, Action? Resume);

    private readonly record struct Outbound(long StreamId, byte[] Buffer, int Length, bool Fin);

    private readonly int _qpackCapacity;

    private H3Connection(QuicConnection quic, IServer server, IEndPoint endPoint, ILogger logger, int qpackCapacity)
    {
        _quic = quic;
        _server = server;
        _endPoint = endPoint;
        _logger = logger;
        _qpackCapacity = qpackCapacity;
    }

    internal static async Task ServeAsync(QuicConnection quic, IServer server, IEndPoint endPoint, ILogger logger,
        int qpackCapacity, CancellationToken cancellationToken)
    {
        await using var connection = new H3Connection(quic, server, endPoint, logger, qpackCapacity);
        await connection.RunAsync(cancellationToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        // Opened before Glyph3 exists, because OpenUniStream answers synchronously while
        // OpenOutboundStreamAsync does not.
        //
        // One is enough at capacity 0: Glyph3 asks for a single unidirectional stream, for control
        // and SETTINGS, and with no dynamic table there is nothing to insert and nothing to
        // acknowledge. With a capacity it also wants the QPACK decoder stream, plus an encoder
        // stream for the case where the client advertises a table of its own. The encoder one goes
        // unused if the client then advertises 0, which every non-browser client does.
        int uniStreams = _qpackCapacity > 0 ? ServerUniStreams + 2 : ServerUniStreams;

        for (int i = 0; i < uniStreams; i++)
        {
            QuicStream uni = await _quic.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, cancellationToken);
            _streams[uni.Id] = uni;
            _spareUniStreams.Enqueue(uni);
        }

        _h3 = new Http3Connection(this, DispatchAsync, new Http3Options
        {
            QpackDynamicTableCapacity = _qpackCapacity,
        });

        Task accepting = AcceptStreamsAsync(cancellationToken);
        Task writing = WriteLoopAsync(cancellationToken);

        try
        {
            await PumpAsync(cancellationToken);
        }
        finally
        {
            ReportQpack();

            _h3.Close();
            _egress.Writer.TryComplete();
            _ingress.Writer.TryComplete();
            await Task.WhenAny(Task.WhenAll(accepting, writing), Task.Delay(1000, CancellationToken.None));
        }
    }

    /// <summary>
    /// Reports what QPACK actually did on this connection, which is otherwise invisible.
    /// </summary>
    /// <remarks>
    /// A capacity of 0 is worth distinguishing from SETTINGS that never arrived, since both leave
    /// the counters at 0 while meaning entirely different things. In practice only browsers
    /// advertise a table at all: everything else sends 0, which makes the dynamic table inert no
    /// matter what capacity this engine was configured with.
    /// </remarks>
    private void ReportQpack()
    {
        if (_h3 is null || !_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        _logger.LogDebug(
            "HTTP/3 connection closed: peer QPACK table {Capacity}, dynamic inserts in {Inbound} / out {Outbound}",
            _h3.PeerSettingsReceived ? $"{_h3.PeerDynamicTableCapacity} B" : "never advertised",
            _h3.InboundDynamicInserts,
            _h3.OutboundDynamicInserts);
    }

    // Glyph3 calls this on the pump thread and awaits the result before submitting the response.
    private ValueTask<Http3Response> DispatchAsync(Http3Request request)
    {
        // Start the handler INLINE. Measured against the alternative: dispatching it to the pool
        // with Task.Run collapsed throughput to 75k req/s from 291k, because the pump then waits on
        // a pool that the offloaded work is itself competing for. A chain that completes synchronously - which the common case
        // does - then costs no thread hop at all, and Glyph3 submits the response without ever
        // leaving the pump. Only a handler that actually suspends pays for a continuation, and it
        // comes back through PumpContext.
        Task<Http3Response> pending = HandleAsync(request);

        return new ValueTask<Http3Response>(pending);
    }

    private async Task<Http3Response> HandleAsync(Http3Request source)
    {
        try
        {
            // Glyph3 dispatches at end-of-headers, so the body is still arriving. Assemble it
            // before the handler runs, which is the shape GenHTTP's IRequestBody expects.
            ReadOnlyMemory<byte> body = await ReadBodyAsync(source);

            var request = new H3Request(_server, _endPoint, source, body, RemoteAddress());

            IResponse response = await _server.Handler.HandleAsync(request)
                                 ?? throw new InvalidOperationException("The root request handler did not return a response");

            bool head = request.Header.Method == RequestMethod.Head;

            return await H3ResponseWriter.BuildAsync(response, head);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to handle request");
            return new Http3Response { Status = 500 };
        }
    }

    private async Task PumpAsync(CancellationToken cancellationToken)
    {
        var context = new PumpContext(_ingress.Writer);

        Enter(context);
        _h3!.Start();
        Leave();

        // ConfigureAwait(false) matters: with the context installed the pump would post its OWN
        // continuation to the queue only it drains, and wait forever for itself.
        while (await _ingress.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            Enter(context);

            while (_ingress.Reader.TryRead(out Inbound item))
            {
                if (item.Resume is { } resume)
                {
                    resume();
                }
                else if (item.Closed)
                {
                    _h3.OnStreamClosed(item.StreamId);
                }
                else
                {
                    _h3.Feed(item.StreamId, item.Buffer.AsSpan(0, item.Length), item.Fin);

                    if (item.Buffer is not null)
                    {
                        ArrayPool<byte>.Shared.Return(item.Buffer);
                    }
                }
            }

            _h3.Flush();

            Leave();

            if (_h3.IsFaulted)
            {
                return;
            }
        }
    }

    // Installed only around calls into Glyph3, so anything it awaits while dispatching resumes on
    // the pump rather than the thread pool.
    private static void Enter(SynchronizationContext context) => SynchronizationContext.SetSynchronizationContext(context);

    private static void Leave() => SynchronizationContext.SetSynchronizationContext(null);

    private static async ValueTask<ReadOnlyMemory<byte>> ReadBodyAsync(Http3Request source)
    {
        if (source.BodyReader is not { } reader)
        {
            return source.Body;
        }

        ArrayBufferWriter<byte>? assembled = null;

        while (true)
        {
            ReadOnlyMemory<byte> chunk = await reader.ReadAsync();

            if (chunk.IsEmpty)
            {
                break;
            }

            assembled ??= new ArrayBufferWriter<byte>(chunk.Length);
            assembled.Write(chunk.Span);
        }

        return assembled?.WrittenMemory ?? ReadOnlyMemory<byte>.Empty;
    }

    private IPAddress? RemoteAddress()
        => _quic.RemoteEndPoint is IPEndPoint endpoint ? endpoint.Address : null;

    private async Task AcceptStreamsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                QuicStream stream = await _quic.AcceptInboundStreamAsync(cancellationToken);
                _streams[stream.Id] = stream;
                _ = ReadStreamAsync(stream, cancellationToken);
            }
        }
        catch (Exception)
        {
            // The connection closed, which is how an accept loop ends.
        }
    }

    private async Task ReadStreamAsync(QuicStream stream, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
                int read = await stream.ReadAsync(buffer, cancellationToken);

                if (read == 0)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    await _ingress.Writer.WriteAsync(new Inbound(stream.Id, null, 0, true, false, null), cancellationToken);
                    return;
                }

                await _ingress.Writer.WriteAsync(new Inbound(stream.Id, buffer, read, false, false, null), cancellationToken);
            }
        }
        catch (Exception)
        {
            _ingress.Writer.TryWrite(new Inbound(stream.Id, null, 0, false, true, null));
        }
    }

    /// <summary>
    /// Issues every write it can, then awaits them together.
    /// </summary>
    /// <remarks>
    /// Awaiting each write before starting the next leaves MsQuic with one stream's data pending at
    /// a time, so it cannot fill a datagram from several streams at once - the coalescing that makes
    /// UDP segmentation offload worth anything. Issuing first and draining after lets it see the
    /// whole batch. Ordering within a stream still holds: a second write to a stream already in
    /// flight drains first.
    /// </remarks>
    private async Task WriteLoopAsync(CancellationToken cancellationToken)
    {
        var inflight = new List<Pending>();
        var busy = new HashSet<long>();

        try
        {
            while (await _egress.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_egress.Reader.TryRead(out Outbound item))
                {
                    if (!_streams.TryGetValue(item.StreamId, out QuicStream? stream))
                    {
                        ArrayPool<byte>.Shared.Return(item.Buffer);
                        continue;
                    }

                    if (item.Length == 0)
                    {
                        if (item.Fin)
                        {
                            stream.CompleteWrites();
                        }

                        ArrayPool<byte>.Shared.Return(item.Buffer);
                        Release(item.StreamId, item.Fin);
                        continue;
                    }

                    if (!busy.Add(item.StreamId))
                    {
                        await DrainAsync(inflight, busy);
                        busy.Add(item.StreamId);
                    }

                    ValueTask write = stream.WriteAsync(item.Buffer.AsMemory(0, item.Length), item.Fin, cancellationToken);

                    if (write.IsCompletedSuccessfully)
                    {
                        ArrayPool<byte>.Shared.Return(item.Buffer);
                        Release(item.StreamId, item.Fin);
                    }
                    else
                    {
                        inflight.Add(new Pending(write, item.Buffer, item.StreamId, item.Fin));
                    }
                }

                await DrainAsync(inflight, busy);
            }
        }
        catch (Exception)
        {
            // Peer went away mid-write.
        }
    }

    private async ValueTask DrainAsync(List<Pending> inflight, HashSet<long> busy)
    {
        foreach (Pending pending in inflight)
        {
            try
            {
                await pending.Write;
            }
            catch (Exception)
            {
                // Peer went away mid-write; the connection is finished either way.
            }

            ArrayPool<byte>.Shared.Return(pending.Buffer);
            Release(pending.StreamId, pending.Fin);
        }

        inflight.Clear();
        busy.Clear();
    }

    /// <summary>
    /// Releases a finished request stream. Skipping this strands its credit and the peer stalls
    /// after MaxInboundBidirectionalStreams requests.
    /// </summary>
    private void Release(long streamId, bool fin)
    {
        // Unidirectional streams are the connection's control and QPACK streams; they live as long
        // as it does.
        if (fin && (streamId & 0x3) == 0x0 && _streams.TryRemove(streamId, out QuicStream? finished))
        {
            // Off the writer: tearing a stream down is slow enough that awaiting it here serialises
            // every other request behind it.
            _ = finished.DisposeAsync().AsTask();
        }
    }

    private readonly record struct Pending(ValueTask Write, byte[] Buffer, long StreamId, bool Fin);

    public long OpenUniStream() => _spareUniStreams.TryDequeue(out QuicStream? stream) ? stream.Id : -1;

    public void Send(long streamId, ReadOnlySpan<byte> data, bool fin)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Max(1, data.Length));
        data.CopyTo(buffer);
        _egress.Writer.TryWrite(new Outbound(streamId, buffer, data.Length, fin));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (QuicStream stream in _streams.Values)
        {
            await stream.DisposeAsync();
        }
        await _quic.DisposeAsync();
    }

    /// <summary>
    /// Posts continuations back to the pump, so anything Glyph3 awaits resumes on the one thread
    /// allowed to touch it.
    /// </summary>
    private sealed class PumpContext : SynchronizationContext
    {
        private readonly ChannelWriter<Inbound> _pump;

        internal PumpContext(ChannelWriter<Inbound> pump) => _pump = pump;

        public override void Post(SendOrPostCallback d, object? state)
            => _pump.TryWrite(new Inbound(0, null, 0, false, false, () => d(state)));

        public override void Send(SendOrPostCallback d, object? state) => Post(d, state);

        public override SynchronizationContext CreateCopy() => this;
    }
}
