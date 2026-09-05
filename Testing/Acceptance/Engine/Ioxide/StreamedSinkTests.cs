using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The response sink for an HTTP/2 stream, which a handler's content writes into a frame at a time,
/// flushing as it goes. One test serves a stream-backed body the ordinary way; the other writes a
/// body through every overload the sink's stream offers, so the sync, async and array paths are all
/// exercised in one response. The response version is asserted to be HTTP/2, so a fall back to
/// HTTP/1.1 - which uses a different sink - fails the test rather than passing it for the wrong reason.
/// </summary>
[TestClass]
public sealed class StreamedSinkTests
{

    [IoxideTestMethod]
    public async Task TestStreamBackedResponseIsStreamedOverHttp2()
    {
        // A stream-backed body is copied to the sink's stream in buffer-sized steps, each flushed -
        // the ordinary way a large response paces itself instead of piling up in memory.
        var payload = RandomBytes(256 * 1024);

        await RunAsync(new StreamResponseHandler(payload), payload);
    }

    [IoxideTestMethod]
    public async Task TestResponseWrittenThroughEveryStreamOverload()
    {
        // A content that writes its body in four segments, one per write overload the sink stream
        // offers, then flushes - so the sync span, sync array, async memory and async array paths are
        // all covered by a single response that still arrives whole and in order.
        var payload = RandomBytes(200_000);

        await RunAsync(new MixedWriteHandler(payload), payload);
    }

    #region Infrastructure

    private static async Task RunAsync(IHandler handler, byte[] expected)
    {
        var port = (ushort)TestHost.NextPort();

        var server = Host.Create()
                         .Handler(handler)
                         .Bind(IPAddress.Loopback, port, HttpProtocols.Http2);

        await server.StartAsync();

        try
        {
            using var socketsHandler = new SocketsHttpHandler();

            using var client = new HttpClient(socketsHandler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };

            using var response = await client.GetAsync($"http://localhost:{port}/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpVersion.Version20, response.Version);

            CollectionAssert.AreEqual(expected, await response.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static byte[] RandomBytes(int length)
    {
        var bytes = new byte[length];
        new Random(42).NextBytes(bytes);
        return bytes;
    }

    /// <summary>Responds with a stream-backed body, which the engine copies into the sink's stream.</summary>
    private sealed class StreamResponseHandler(byte[] payload) : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
            => new(request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(new MemoryStream(payload), ContentType.ApplicationOctetStream)
                          .Build());
    }

    /// <summary>Responds with content that drives every write overload of the sink's stream.</summary>
    private sealed class MixedWriteHandler(byte[] payload) : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
            => new(request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(new MixedWriteContent(payload))
                          .Build());
    }

    /// <summary>
    /// Writes its body in four contiguous segments - one each through the sync span, sync array, async
    /// memory and async array overloads - then flushes. The segments concatenate back to the payload.
    /// </summary>
    private sealed class MixedWriteContent(byte[] payload) : IResponseContent
    {
        public ulong? Length => (ulong)payload.Length;

        public ContentType? Type => ContentType.ApplicationOctetStream;

        public ReadOnlyMemory<byte>? Encoding => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong?)null);

        public async ValueTask WriteAsync(IResponseSink sink)
        {
            var stream = sink.Stream;

            var quarter = payload.Length / 4;

            stream.Write(payload.AsSpan(0, quarter));                                        // sync span
            stream.Write(payload, quarter, quarter);                                         // sync array
            stream.Flush();                                                                  // no-op, but a content may call it

            await stream.WriteAsync(payload.AsMemory(2 * quarter, quarter));                 // async memory
            await stream.WriteAsync(payload, 3 * quarter, payload.Length - 3 * quarter);     // async array (remainder)

            await stream.FlushAsync();
        }
    }

    #endregion

}
