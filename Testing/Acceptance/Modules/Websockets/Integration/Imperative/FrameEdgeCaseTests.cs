using System.Text;

using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

using GenHTTP.Testing.Acceptance.Modules.Websockets.RawClient;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Imperative;

[TestClass]
public sealed class FrameEdgeCaseTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPingInterruptsSegmentedMessage(TestEngine engine)
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
    public async Task TestCloseInterruptsSegmentedMessage(TestEngine engine)
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

        await using var host = await TestHost.RunAsync(websocket, engine: TestEngine.Internal);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        var frame = RawWebSocketClient.BuildClientFrame("hello"u8.ToArray(), opcode: 0x1, fin: true);

        // header + mask only (6 bytes for a small payload) - the payload never arrives
        var partial = frame[..6];

        await client.SendRawInChunksAsync(partial, chunkSize: partial.Length);

        client.ShutdownWrite();

        Assert.AreEqual(FrameError.UnexpectedEndOfStream, await client.ReceiveTextFrameAsync());
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
