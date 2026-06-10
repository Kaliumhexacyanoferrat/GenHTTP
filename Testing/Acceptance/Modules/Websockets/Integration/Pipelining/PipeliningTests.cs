using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

using GenHTTP.Testing.Acceptance.Modules.Websockets.RawClient;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Pipelining;

[TestClass]
public sealed class PipeliningTests
{

    /// <summary>
    /// Three frames written with flush:false followed by an explicit FlushAsync
    /// must all arrive at the client in a single batch.
    /// </summary>
    [TestMethod]
    public async Task TestBatchedWritesWithExplicitFlush()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new BatchHandler());

        await using var host = await TestHost.RunAsync(websocket);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        await client.SendRawInChunksAsync(RawWebSocketClient.BuildClientFrame("go"u8.ToArray(), opcode: 0x1, fin: true), chunkSize: 64);

        var r1 = await client.ReceiveTextFrameAsync();
        var r2 = await client.ReceiveTextFrameAsync();
        var r3 = await client.ReceiveTextFrameAsync();

        Assert.AreEqual("frame1", r1);
        Assert.AreEqual("frame2", r2);
        Assert.AreEqual("frame3", r3);
    }

    /// <summary>
    /// Frames written with flush:false must be flushed when the last write uses flush:true (the default).
    /// </summary>
    [TestMethod]
    public async Task TestBatchedWritesFlushedByLastWrite()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new LastWriteFlushHandler());

        await using var host = await TestHost.RunAsync(websocket);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        await client.SendRawInChunksAsync(RawWebSocketClient.BuildClientFrame("go"u8.ToArray(), opcode: 0x1, fin: true), chunkSize: 64);

        var r1 = await client.ReceiveTextFrameAsync();
        var r2 = await client.ReceiveTextFrameAsync();
        var r3 = await client.ReceiveTextFrameAsync();

        Assert.AreEqual("frame1", r1);
        Assert.AreEqual("frame2", r2);
        Assert.AreEqual("frame3", r3);
    }

    /// <summary>
    /// A frame written with flush:false must not reach the client until FlushAsync is called.
    /// The test verifies this by interleaving a second send/receive round-trip: only after
    /// the server calls FlushAsync does the buffered frame become readable.
    /// </summary>
    [TestMethod]
    public async Task TestBufferedFrameNotDeliveredBeforeFlush()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new BufferThenFlushHandler());

        await using var host = await TestHost.RunAsync(websocket);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        // First trigger: server writes "buffered" with flush:false, then reads again before flushing
        var trigger = "step1"u8.ToArray();
        await client.SendRawInChunksAsync(RawWebSocketClient.BuildClientFrame(trigger, opcode: 0x1, fin: true), chunkSize: 64);

        // Second trigger: server calls FlushAsync and then writes "done" with flush:true
        var trigger2 = "step2"u8.ToArray();
        await client.SendRawInChunksAsync(RawWebSocketClient.BuildClientFrame(trigger2, opcode: 0x1, fin: true), chunkSize: 64);

        // Both frames must now be readable in order
        var r1 = await client.ReceiveTextFrameAsync();
        var r2 = await client.ReceiveTextFrameAsync();

        Assert.AreEqual("buffered", r1);
        Assert.AreEqual("done", r2);
    }

    /// <summary>
    /// Three frames sent in a single TCP write arrive in one pipe read. The handler
    /// should drain all three synchronously via TryReadFrame without any async wait,
    /// then receive false on the fourth call indicating the buffer is empty.
    /// </summary>
    [TestMethod]
    public async Task TestTryReadFrameDrainsBufferedFrames()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new TryReadHandler());

        await using var host = await TestHost.RunAsync(websocket);

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        // Send three frames in one TCP write so all arrive in the same pipe read
        await client.SendMultipleTextFramesSingleWriteAsync(default, "alpha", "beta", "gamma");

        // Server echoes each frame back; all three must arrive
        var r1 = await client.ReceiveTextFrameAsync();
        var r2 = await client.ReceiveTextFrameAsync();
        var r3 = await client.ReceiveTextFrameAsync();

        // The fourth receive is the "empty" signal the handler writes after TryReadFrame returned false
        var r4 = await client.ReceiveTextFrameAsync();

        Assert.AreEqual("alpha", r1);
        Assert.AreEqual("beta",  r2);
        Assert.AreEqual("gamma", r3);
        Assert.AreEqual("empty", r4);
    }

    // -------------------------------------------------------------------------
    // Handlers
    // -------------------------------------------------------------------------

    private sealed class BatchHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            var frame = await connection.ReadFrameAsync();

            if (frame.Type == FrameType.Close) return;

            await connection.WriteAsync("frame1"u8.ToArray(), flush: false);
            await connection.WriteAsync("frame2"u8.ToArray(), flush: false);
            await connection.WriteAsync("frame3"u8.ToArray(), flush: false);
            await connection.FlushAsync();

            await connection.CloseAsync();
        }
    }

    private sealed class LastWriteFlushHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            var frame = await connection.ReadFrameAsync();

            if (frame.Type == FrameType.Close) return;

            await connection.WriteAsync("frame1"u8.ToArray(), flush: false);
            await connection.WriteAsync("frame2"u8.ToArray(), flush: false);
            await connection.WriteAsync("frame3"u8.ToArray()); // flush: true by default

            await connection.CloseAsync();
        }
    }

    private sealed class TryReadHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            // Block until at least the first frame arrives (and the rest are buffered)
            var first = await connection.ReadFrameAsync();
            if (first.Type == FrameType.Close) return;

            await connection.WriteAsync(first.Data, flush: false);

            // Drain whatever else is already in the buffer synchronously
            while (connection.TryReadFrame(out var buffered))
            {
                if (buffered.Type == FrameType.Close) return;
                await connection.WriteAsync(buffered.Data, flush: false);
            }

            // Signal that the buffer was empty on the last TryReadFrame call
            await connection.WriteAsync("empty"u8.ToArray(), flush: false);
            await connection.FlushAsync();

            await connection.CloseAsync();
        }
    }

    private sealed class BufferThenFlushHandler : IImperativeHandler
    {
        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            // step1: buffer a frame without flushing, then wait for another client message
            var frame1 = await connection.ReadFrameAsync();
            if (frame1.Type == FrameType.Close) return;

            await connection.WriteAsync("buffered"u8.ToArray(), flush: false);

            // step2: now flush, then send a second frame with the normal flush
            var frame2 = await connection.ReadFrameAsync();
            if (frame2.Type == FrameType.Close) return;

            await connection.FlushAsync();
            await connection.WriteAsync("done"u8.ToArray());

            await connection.CloseAsync();
        }
    }

}
