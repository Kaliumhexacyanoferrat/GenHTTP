using System.Buffers;
using System.IO.Pipelines;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

using NSubstitute;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public sealed class ConnectionTests
{

    [TestMethod]
    public async Task TestReadFailureAfterFrameIsReported()
    {
        var pipe = CreatePipe();

        await using var connection = CreateConnection(pipe.Reader);

        // the frame ends where an empty segment begins, as with a transport that asks for a large
        // span per read and gets nothing back (ioxide's TLS reader does this)
        pipe.Writer.Write(MaskedText("hello"));
        pipe.Writer.GetSpan(16 * 1024);
        await pipe.Writer.FlushAsync();

        var frame = await connection.ReadFrameAsync();

        Assert.AreEqual(FrameType.Text, frame.Type);
        Assert.AreEqual("hello", Encoding.UTF8.GetString(frame.Data.Span));

        await pipe.Writer.CompleteAsync(new IOException("transport failed"));

        var e = await Assert.ThrowsExactlyAsync<IOException>(() => connection.ReadFrameAsync().AsTask());

        Assert.AreEqual("transport failed", e.Message);
    }

    [TestMethod]
    public async Task TestReadFailureBeforeFirstFrameIsReported()
    {
        var pipe = CreatePipe();

        await using var connection = CreateConnection(pipe.Reader);

        await pipe.Writer.CompleteAsync(new IOException("transport failed"));

        var e = await Assert.ThrowsExactlyAsync<IOException>(() => connection.ReadFrameAsync().AsTask());

        Assert.AreEqual("transport failed", e.Message);
    }

    private static Pipe CreatePipe() => new(new PipeOptions(readerScheduler: PipeScheduler.Inline, writerScheduler: PipeScheduler.Inline, useSynchronizationContext: false));

    private static WebsocketConnection CreateConnection(PipeReader reader)
    {
        var server = Substitute.For<IServer>();
        server.Running.Returns(true);

        var request = Substitute.For<IRequest>();
        request.Server.Returns(server);
        request.Upgrade().Returns(reader);

        var settings = new ConnectionSettings(Formatting.Default().Build(), new JsonFormat(), HandleContinuationFramesManually: false, AllocateFrameData: true);

        return new WebsocketConnection(request, Substitute.For<IResponseSink>(), settings);
    }

    private static byte[] MaskedText(string text)
    {
        var payload = Encoding.UTF8.GetBytes(text);

        byte[] mask = [1, 2, 3, 4];

        var frame = new byte[6 + payload.Length];

        frame[0] = 0x81; // FIN, text
        frame[1] = (byte)(0x80 | payload.Length); // masked, 7 bit length

        mask.CopyTo(frame, 2);

        for (var i = 0; i < payload.Length; i++)
        {
            frame[6 + i] = (byte)(payload[i] ^ mask[i & 3]);
        }

        return frame;
    }

}
