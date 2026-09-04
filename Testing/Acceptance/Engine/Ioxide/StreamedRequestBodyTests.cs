using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The request body that arrives on an HTTP/2 stream, which the engine hands to the handler a chunk
/// at a time rather than buffering. Each test posts a body and has the handler read it back a
/// different way, so both the assemble-it-whole and the read-it-as-a-stream paths are exercised
/// end to end. The response version is asserted to be HTTP/2, so a fallback to HTTP/1.1 - which uses
/// a different body - would fail the test rather than pass it for the wrong reason.
/// </summary>
[TestClass]
public sealed class StreamedRequestBodyTests
{

    [IoxideTestMethod]
    public async Task TestSmallBodyIsReadWhole()
    {
        // One short body, read via AsMemoryAsync: a chunk or two, then the empty read that ends it.
        await RunEchoAsync(ReadWhole, "Hello, streamed body!"u8.ToArray());
    }

    [IoxideTestMethod]
    public async Task TestLargeBodyIsAssembledFromManyChunks()
    {
        // Large enough that the stream arrives in several chunks, so AsMemoryAsync loops to assemble it.
        await RunEchoAsync(ReadWhole, RandomBytes(256 * 1024));
    }

    [IoxideTestMethod]
    public async Task TestBodyIsReadInSmallStreamSteps()
    {
        // A 64-byte buffer against a large body drains each chunk over several reads and fetches the
        // next only when it runs out - the PullStream partial-read path, through the Memory<byte> overload.
        await RunEchoAsync(ReadInSmallSteps, RandomBytes(256 * 1024));
    }

    [IoxideTestMethod]
    public async Task TestBodyIsReadThroughTheArrayOverload()
    {
        // The same streaming, but driven through the byte[] overload of ReadAsync rather than Memory<byte>.
        await RunEchoAsync(ReadThroughArrayOverload, RandomBytes(128 * 1024));
    }

    #region Read strategies

    // Reads the body whole, exercising AsMemoryAsync and its chunk-assembly loop.
    private static async ValueTask<byte[]> ReadWhole(IRequestBody body)
        => (await body.AsMemoryAsync()).ToArray();

    // Reads the body through its Stream in small steps, so a chunk is drained across several reads
    // and the next is fetched only when it runs out - the PullStream path via the Memory<byte> overload.
    private static async ValueTask<byte[]> ReadInSmallSteps(IRequestBody body)
    {
        await using var stream = body.AsStream();

        var assembled = new MemoryStream();
        var buffer = new byte[64];

        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            assembled.Write(buffer, 0, read);
        }

        return assembled.ToArray();
    }

    // The same, but through the byte[] overload of ReadAsync rather than the Memory<byte> one.
    private static async ValueTask<byte[]> ReadThroughArrayOverload(IRequestBody body)
    {
        await using var stream = body.AsStream();

        var assembled = new MemoryStream();
        var buffer = new byte[128];

        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            assembled.Write(buffer, 0, read);
        }

        return assembled.ToArray();
    }

    #endregion

    #region Infrastructure

    private static async Task RunEchoAsync(Func<IRequestBody, ValueTask<byte[]>> read, byte[] payload)
    {
        var port = (ushort)TestHost.NextPort();

        var server = Host.Create()
                         .Handler(new EchoHandler(read))
                         .Bind(IPAddress.Loopback, port, HttpProtocols.Http2);

        await server.StartAsync();

        try
        {
            using var handler = new SocketsHttpHandler();

            using var client = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };

            using var content = new ByteArrayContent(payload);

            using var response = await client.PostAsync($"http://localhost:{port}/", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpVersion.Version20, response.Version);

            CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
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

    /// <summary>Reads the request body with the given strategy and echoes it straight back.</summary>
    private sealed class EchoHandler(Func<IRequestBody, ValueTask<byte[]>> read) : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var body = request.GetBody();

            if (body is null)
            {
                return request.Respond().Status(ResponseStatus.BadRequest).Build();
            }

            var bytes = await read(body);

            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(bytes, ContentType.ApplicationOctetStream)
                          .Build();
        }
    }

    #endregion

}
