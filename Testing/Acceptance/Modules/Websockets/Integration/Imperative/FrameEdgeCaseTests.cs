using System.Buffers.Binary;
using System.Text;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

using GenHTTP.Testing.Acceptance.Modules.Websockets.RawClient;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Imperative;

[TestClass]
public sealed class FrameEdgeCaseTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPingInterruptsSegmentedMessage(ServerEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new EchoAllHandler());

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        // Text(fin=0) + Ping(fin=1) interleaved + Continuation(fin=1) completing the message
        var first = RawWebSocketClient.BuildClientFrame("Hello "u8.ToArray(), opcode: 0x1, fin: false);
        var ping = RawWebSocketClient.BuildClientFrame("ping-data"u8.ToArray(), opcode: 0x9, fin: true);
        var last = RawWebSocketClient.BuildClientFrame("World"u8.ToArray(), opcode: 0x0, fin: true);

        var combined = first.Concat(ping).Concat(last).ToArray();
        await client.SendRawInChunksAsync(combined, chunkSize: combined.Length);

        var (pingResponseOpcode, _, pingResponsePayload) = await client.ReceiveFrameAsync();

        Assert.AreEqual((byte)0xA, pingResponseOpcode);
        Assert.AreEqual("ping-data", Encoding.UTF8.GetString(pingResponsePayload));

        Assert.AreEqual("Hello World", await client.ReceiveTextFrameAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCloseInterruptsSegmentedMessage(ServerEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new EchoAllHandler());

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        // Text(fin=0) followed directly by a Close - the in-progress segmented message is abandoned
        var first = RawWebSocketClient.BuildClientFrame("Hello "u8.ToArray(), opcode: 0x1, fin: false);
        var close = RawWebSocketClient.BuildClientFrame([], opcode: 0x8, fin: true);

        var combined = first.Concat(close).ToArray();
        await client.SendRawInChunksAsync(combined, chunkSize: combined.Length);

        var (opcode, _, _) = await client.ReceiveFrameAsync();

        Assert.AreEqual((byte)0x8, opcode);
    }

    [TestMethod]
    public async Task TestTruncatedFrameAtEofYieldsError()
    {
        // Internal only: Kestrel tears down the connection outright on a half-closed upgrade
        // stream instead of letting the handler write a response - a deeper ASP.NET Core
        // transport behavior, not something specific to this (engine-agnostic) code path.
        var websocket = GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new EchoAllHandler());

        await using var host = await TestHost.RunAsync(websocket, engine: ServerEngine.Internal);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        var frame = RawWebSocketClient.BuildClientFrame("hello"u8.ToArray(), opcode: 0x1, fin: true);

        // header + mask only (6 bytes for a small payload) - the payload never arrives
        var partial = frame[..6];

        await client.SendRawInChunksAsync(partial, chunkSize: partial.Length);

        client.ShutdownWrite();

        Assert.AreEqual(FrameError.UnexpectedEndOfStream, await client.ReceiveTextFrameAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDrainedPingKeepsSegmentedMessage(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new DrainingEchoHandler()), engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Arriving in one read, the ping and the final fragment are both drained with TryReadFrame
        var combined = RawWebSocketClient.BuildClientFrame("Hello "u8, opcode: 0x1, fin: false)
                                         .Concat(RawWebSocketClient.BuildClientFrame("ping-data"u8, opcode: 0x9, fin: true))
                                         .Concat(RawWebSocketClient.BuildClientFrame("World"u8, opcode: 0x0, fin: true))
                                         .ToArray();

        await client.SendRawInChunksAsync(combined, chunkSize: combined.Length, timeout.Token);

        var (opcode, _, payload) = await client.ReceiveFrameAsync(timeout.Token);

        Assert.AreEqual((byte)0xA, opcode);
        Assert.AreEqual("ping-data", Encoding.UTF8.GetString(payload));

        Assert.AreEqual("Hello World", await client.ReceiveTextFrameAsync(timeout.Token));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDrainedFragmentedMessageIsReassembled(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new DrainingEchoHandler()), engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var combined = RawWebSocketClient.BuildClientFrame("one"u8, opcode: 0x1, fin: true)
                                         .Concat(RawWebSocketClient.BuildClientFrame("Hello "u8, opcode: 0x1, fin: false))
                                         .Concat(RawWebSocketClient.BuildClientFrame("World"u8, opcode: 0x0, fin: true))
                                         .ToArray();

        await client.SendRawInChunksAsync(combined, chunkSize: combined.Length, timeout.Token);

        Assert.AreEqual("one", await client.ReceiveTextFrameAsync(timeout.Token));
        Assert.AreEqual("Hello World", await client.ReceiveTextFrameAsync(timeout.Token));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalidUtf8FailsConnection(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new DrainingEchoHandler()), engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // 0xC3 starts a two-byte sequence that 0x28 does not continue: each fragment alone could still be valid
        var combined = RawWebSocketClient.BuildClientFrame([0xC3], opcode: 0x1, fin: false)
                                         .Concat(RawWebSocketClient.BuildClientFrame([0x28], opcode: 0x0, fin: true))
                                         .ToArray();

        await client.SendRawInChunksAsync(combined, chunkSize: combined.Length, timeout.Token);

        Assert.AreEqual(1007, await ReceiveCloseStatusAsync(client, timeout.Token));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContinuationWithoutMessageFailsConnection(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(GenHTTP.Modules.Websockets.Websocket.Imperative().Handler(new DrainingEchoHandler()), engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var frame = RawWebSocketClient.BuildClientFrame("orphan"u8, opcode: 0x0, fin: true);

        await client.SendRawInChunksAsync(frame, chunkSize: frame.Length, timeout.Token);

        Assert.AreEqual(1002, await ReceiveCloseStatusAsync(client, timeout.Token));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMaxFrameSizeAcceptsLargerFrames(ServerEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Imperative()
                                                            .MaxFrameSize(64 * 1024)
                                                            .Handler(new DrainingEchoHandler());

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var message = new string('a', 64 * 1024);
        var frame = RawWebSocketClient.BuildClientFrame(Encoding.UTF8.GetBytes(message), opcode: 0x1, fin: true);

        await client.SendRawInChunksAsync(frame, chunkSize: frame.Length, timeout.Token);

        Assert.AreEqual(message, await client.ReceiveTextFrameAsync(timeout.Token));
    }

    private static async Task<ushort> ReceiveCloseStatusAsync(RawWebSocketClient client, CancellationToken token)
    {
        var (opcode, _, payload) = await client.ReceiveFrameAsync(token);

        Assert.AreEqual((byte)0x8, opcode);

        return BinaryPrimitives.ReadUInt16BigEndian(payload);
    }

    /// <summary>
    /// Reads the way a high-throughput handler does: one awaited read, then every frame already
    /// buffered with TryReadFrame, then one flush. Protocol errors close with their status code.
    /// </summary>
    private sealed class DrainingEchoHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            while (true)
            {
                var frame = await connection.ReadFrameAsync();

                if (!await HandleAsync(connection, frame)) return;

                while (connection.TryReadFrame(out frame))
                {
                    if (!await HandleAsync(connection, frame)) return;
                }

                await connection.FlushAsync();
            }
        }

        private static async ValueTask<bool> HandleAsync(IImperativeConnection connection, IWebsocketFrame frame)
        {
            if (frame.IsError(out var error))
            {
                var status = error.CloseStatus;

                if (status != null)
                {
                    await connection.CloseAsync(statusCode: status.Value);
                }

                return false;
            }

            switch (frame.Type)
            {
                case FrameType.Close:
                    await connection.CloseAsync();
                    return false;

                case FrameType.Ping:
                    await connection.PongAsync(frame.Data, flush: false);
                    return true;

                case FrameType.Text or FrameType.Binary:
                    await connection.WriteAsync(frame.Data, frame.Type, flush: false);
                    return true;

                default:
                    return true;
            }
        }
    }

    private sealed class EchoAllHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            while (true)
            {
                var frame = await connection.ReadFrameAsync();

                if (frame.Type == FrameType.Close)
                {
                    await connection.CloseAsync();
                    return;
                }

                if (frame.Type == FrameType.Ping)
                {
                    await connection.PongAsync(frame.Data);
                    continue;
                }

                if (frame.IsError(out var error))
                {
                    await connection.WriteAsync(Encoding.UTF8.GetBytes(error.Message));
                    await connection.CloseAsync();
                    return;
                }

                await connection.WriteAsync(frame.Data);
            }
        }
    }

}
