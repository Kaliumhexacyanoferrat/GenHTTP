using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

[TestClass]
public sealed class DecompressionTests
{
    private const string TestContent = "Hello, this is test content that will be compressed!";

    /// <summary>
    /// As a developer, I expect incoming gzip-compressed requests to be automatically decompressed.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGzipDecompression(TestEngine engine)
    {
        await TestDecompressionAsync(engine, "gzip", CompressGzip);
    }

    /// <summary>
    /// As a developer, I expect incoming brotli-compressed requests to be automatically decompressed.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestBrotliDecompression(TestEngine engine)
    {
        await TestDecompressionAsync(engine, "br", CompressBrotli);
    }

    /// <summary>
    /// As a developer, I expect incoming zstd-compressed requests to be automatically decompressed.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestZstdDecompression(TestEngine engine)
    {
        await TestDecompressionAsync(engine, "zstd", CompressZstd);
    }

    /// <summary>
    /// As a developer, I expect requests without Content-Encoding to pass through unchanged.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoDecompressionWithoutHeader(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            using var reader = new StreamReader(r.Content!);
            var body = reader.ReadToEnd();

            return r.Respond()
                    .Content(Resource.FromString(body).Build())
                    .Type(ContentType.TextPlain)
                    .Build();
        });

        await using var runner = new TestHost(handler.Wrap().Build(), defaults: false, engine: engine);

        await runner.Host.Decompression(DecompressedContent.Default()).StartAsync();

        var request = runner.GetRequest(method: HttpMethod.Post);
        request.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

        using var response = await runner.GetResponseAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(TestContent, responseBody);
    }

    /// <summary>
    /// As a developer, I expect requests with unknown Content-Encoding to pass through unchanged.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoDecompressionWithUnknownEncoding(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            using var reader = new StreamReader(r.Content!);
            var body = reader.ReadToEnd();

            return r.Respond()
                    .Content(Resource.FromString(body).Build())
                    .Type(ContentType.TextPlain)
                    .Build();
        });

        await using var runner = new TestHost(handler.Wrap().Build(), defaults: false, engine: engine);

        await runner.Host.Decompression(DecompressedContent.Default()).StartAsync();

        var request = runner.GetRequest(method: HttpMethod.Post);
        request.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");
        request.Content.Headers.ContentEncoding.Add("unknown-encoding");

        using var response = await runner.GetResponseAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(TestContent, responseBody);
    }

    /// <summary>
    /// As a developer, I want to add custom decompression algorithms.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomDecompressionAlgorithm(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            using var reader = new StreamReader(r.Content!);
            var body = reader.ReadToEnd();

            return r.Respond()
                    .Content(Resource.FromString(body).Build())
                    .Type(ContentType.TextPlain)
                    .Build();
        });

        await using var runner = new TestHost(handler.Wrap().Build(), defaults: false, engine: engine);

        await runner.Host.Decompression(DecompressedContent.Empty().Add(new CustomDecompression())).StartAsync();

        var request = runner.GetRequest(method: HttpMethod.Post);
        // "custom" encoding just reverses the string
        var reversed = new string(TestContent.Reverse().ToArray());
        request.Content = new StringContent(reversed, Encoding.UTF8, "text/plain");
        request.Content.Headers.ContentEncoding.Add("custom");

        using var response = await runner.GetResponseAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(TestContent, responseBody);
    }

    /// <summary>
    /// As a developer, I expect decompression to work with the Defaults() method.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestDecompressionViaDefaults(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            using var reader = new StreamReader(r.Content!);
            var body = reader.ReadToEnd();

            return r.Respond()
                    .Content(Resource.FromString(body).Build())
                    .Type(ContentType.TextPlain)
                    .Build();
        });

        await using var runner = new TestHost(handler.Wrap().Build(), defaults: false, engine: engine);

        await runner.Host.Defaults(compression: false, decompression: true, secureUpgrade: false, strictTransport: false, clientCaching: false).StartAsync();

        var request = GetRequestWithCompressedPayload(runner);

        using var response = await runner.GetResponseAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(TestContent, responseBody);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestWrapping(TestEngine engine)
    {
        var app = Inline.Create()
                        .Post((IRequest wrapped) =>
                        {
                            Assert.IsNotNull(wrapped.Properties);
                            Assert.IsNotNull(wrapped.Server);
                            Assert.IsNotNull(wrapped.Client);
                            Assert.IsNotNull(wrapped.LocalClient);
                            Assert.IsNotNull(wrapped.Target);
                            Assert.IsNotNull(wrapped.Cookies);
                            Assert.IsNotNull(wrapped.Forwardings);

                            Assert.AreEqual(HttpProtocol.Http11, wrapped.ProtocolType);
                            Assert.AreEqual(RequestMethod.Post, wrapped.Method.KnownMethod);

                            Assert.AreEqual("https://google.com/", wrapped.Referer);
                            Assert.AreEqual("server.local", wrapped.Host);
                            Assert.AreEqual("test-client/1.0 (100% not compatible)", wrapped.UserAgent);

                            Assert.AreEqual("1", wrapped.Query["x"]);
                            Assert.AreEqual("test-client/1.0 (100% not compatible)", wrapped.Headers["User-Agent"]);

                            return wrapped.Respond()
                                          .Status(ResponseStatus.NoContent);
                        });

        await using var runner = new TestHost(app.Build(), defaults: false, engine: engine);

        await runner.Host.Defaults(decompression: true).StartAsync();

        var request = GetRequestWithCompressedPayload(runner, "/?x=1");

        request.Headers.Add("Referer", "https://google.com");
        request.Headers.Add("Host", "server.local");
        request.Headers.Add("User-Agent", "test-client/1.0 (100% not compatible)");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    #region Helpers

    private static HttpRequestMessage GetRequestWithCompressedPayload(TestHost runner, string? path = null)
    {
        var compressedContent = CompressGzip(TestContent);

        var request = runner.GetRequest(path, HttpMethod.Post);

        request.Content = new ByteArrayContent(compressedContent);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        request.Content.Headers.ContentEncoding.Add("gzip");

        return request;
    }

    private static async Task TestDecompressionAsync(TestEngine engine, string encoding, Func<string, byte[]> compressor)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            using var reader = new StreamReader(r.Content!);
            var body = reader.ReadToEnd();

            return r.Respond()
                    .Content(Resource.FromString(body).Build())
                    .Type(ContentType.TextPlain)
                    .Build();
        });

        await using var runner = new TestHost(handler.Wrap().Build(), defaults: false, engine: engine);

        await runner.Host.Decompression(DecompressedContent.Default()).StartAsync();

        var compressedContent = compressor(TestContent);

        var request = runner.GetRequest(method: HttpMethod.Post);
        request.Content = new ByteArrayContent(compressedContent);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        request.Content.Headers.ContentEncoding.Add(encoding);

        using var response = await runner.GetResponseAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(TestContent, responseBody);
    }

    private static byte[] CompressGzip(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    private static byte[] CompressBrotli(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            brotli.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    private static byte[] CompressZstd(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var output = new MemoryStream();
        using (var zstd = new ZstdSharp.CompressionStream(output, 1, leaveOpen: true))
        {
            zstd.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    #endregion

    #region Custom Algorithm

    private class CustomDecompression : ICompressionAlgorithm
    {
        public string Name => "custom";

        public Priority Priority => Priority.Low;

        public IResponseContent Compress(IResponseContent content, CompressionLevel level) => throw new NotImplementedException();

        public Stream Decompress(Stream content)
        {
            // Simple "decompression" that reverses the bytes
            using var reader = new StreamReader(content);
            var reversed = new string(reader.ReadToEnd().Reverse().ToArray());
            return new MemoryStream(Encoding.UTF8.GetBytes(reversed));
        }

    }

    #endregion

}
