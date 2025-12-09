using System.IO.Compression;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

using GenHTTP.Testing.Acceptance.Utilities;

using Strings = GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

[TestClass]
public sealed class CompressionTests
{

    /// <summary>
    /// As a developer, I expect responses to be compressed out of the box.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompression(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(CreateLargeContentHandler().Build(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("zstd", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a browser, I expect only supported compression algorithms to be used
    /// to generate my response.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestSpecificAlgorithms(TestEngine engine)
    {
        foreach (var algorithm in new[] { "gzip", "br", "zstd" })
        {
            await using var runner = await TestHost.RunAsync(CreateLargeContentHandler(), engine: engine);

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", algorithm);

            using var response = await runner.GetResponseAsync(request);

            Assert.AreEqual(algorithm, response.Content.Headers.ContentEncoding.First());
        }
    }

    /// <summary>
    /// As a developer, I want to be able to disable compression.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionDisabled(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), false, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.IsEmpty(response.Content.Headers.ContentEncoding);
    }

    /// <summary>
    /// As a developer, I want to be able to add custom compression algorithms.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomCompression(TestEngine engine)
    {
        await using var runner = new TestHost(CreateLargeContentHandler().Build(), engine: engine);

        await runner.Host.Compression(CompressedContent.Default().Add(new CustomAlgorithm()).Level(CompressionLevel.Optimal)).StartAsync();

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "custom");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("custom", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a developer, I want already compressed content not to be compressed again.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoAdditionalCompression(TestEngine engine)
    {
        var image = Resource.FromString("Image!").Type(ContentType.ImageJpg);

        await using var runner = await TestHost.RunAsync(Layout.Create().Add("uncompressed", Content.From(image)), engine: engine);

        using var response = await runner.GetResponseAsync("/uncompressed");

        Assert.IsEmpty(response.Content.Headers.ContentEncoding);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVariyHeaderAdded(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(CreateLargeContentHandler(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("Accept-Encoding", response.GetHeader("Vary"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVaryHeaderExtendedAdded(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Vary", "Host")
                    .Content(Resource.FromString(CreateLargeString(500)).Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip");

        using var response = await runner.GetResponseAsync(request);

        Assert.Contains("Host", response.Headers.Vary);
        Assert.Contains("Accept-Encoding", response.Headers.Vary);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentType(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString(CreateLargeString(500)).Build())
                    .Type("application/json; charset=utf-8")
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("br", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a developer, I want content smaller than a threshold not to be compressed to avoid unnecessary overhead.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_SmallContent_NotCompressed(TestEngine engine)
    {
        // Creating content smaller than the default threshold (256 bytes)
        var smallContent = CreateLargeString(100);

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString(smallContent).Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should not be compressed as it's below the threshold
        Assert.IsEmpty(response.Content.Headers.ContentEncoding);
    }

    /// <summary>
    /// As a developer, I want content larger than a threshold to be compressed.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_LargeContent_Compressed(TestEngine engine)
    {
        // Creating content larger than the default threshold (256 bytes)
        var largeContent = CreateLargeString(500);

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString(largeContent).Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should be compressed as it's above the threshold
        Assert.IsNotEmpty(response.Content.Headers.ContentEncoding);
        Assert.AreEqual("zstd", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a developer, I want content exactly at the threshold to be compressed (>= check).
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_ExactThreshold_Compressed(TestEngine engine)
    {
        // Creating content exactly at the default threshold (256 bytes)
        var exactContent = CreateLargeString(256);

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString(exactContent).Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should be compressed as it's exactly at the threshold (>= check)
        Assert.IsNotEmpty(response.Content.Headers.ContentEncoding);
        Assert.AreEqual("zstd", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a developer, I want to be able to configure a custom minimum size threshold.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_CustomThreshold(TestEngine engine)
    {
        // Creating content well below the threshold to ensure it's not compressed
        var smallContent = CreateLargeString(100);

        var handler = Content.From(Resource.FromString(smallContent).Type(ContentType.TextHtml));

        var runner = new TestHost(handler.Build(), engine: engine);

        // Set custom threshold to 500 bytes - content should NOT be compressed
        await runner.Host.Compression(CompressedContent.Default().MinimumSize(500)).StartAsync();

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should not be compressed as it's below the custom threshold (500 bytes)
        Assert.IsEmpty(response.Content.Headers.ContentEncoding);

        await runner.DisposeAsync();
    }

    /// <summary>
    /// As a developer, I want content with unknown length to always be compressed if suitable.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_UnknownLength_AlwaysCompressed(TestEngine engine)
    {
        // Creating a custom content provider that returns null for length
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            var wrappedContent = new UnknownLengthContent(new Strings.StringContent("Small content"));

            return r.Respond()
                    .Content(wrappedContent)
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content with unknown length should always be compressed if suitable
        Assert.IsNotEmpty(response.Content.Headers.ContentEncoding);
        Assert.AreEqual("zstd", response.Content.Headers.ContentEncoding.First());
    }

    /// <summary>
    /// As a developer, I want to be able to chain MinimumSize with Add and Level methods.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_MethodChaining(TestEngine engine)
    {
        // Verifying that the example usage pattern works: CompressedContent.Default().MinimumSize(512)
        var smallContent = CreateLargeString(100);

        var handler = Content.From(Resource.FromString(smallContent).Type(ContentType.TextHtml));

        var runner = new TestHost(handler.Build(), engine: engine);

        await runner.Host.Compression(CompressedContent.Default()
                                                       .MinimumSize(512)
                                                       .Add(new CustomAlgorithm())
                                                       .Level(CompressionLevel.Optimal))
                     .StartAsync();

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "custom, gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should not be compressed as it's below the threshold (100 < 512)
        Assert.IsEmpty(response.Content.Headers.ContentEncoding);

        await runner.DisposeAsync();
    }

    /// <summary>
    /// As a developer, I want to be able to disable the threshold by setting it to null.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompressionThreshold_Disabled_AllContentCompressed(TestEngine engine)
    {
        // Creating very small content
        var smallContent = CreateLargeString(10); // 10 bytes when UTF-8 encoded

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString(smallContent).Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        var runner = new TestHost(handler.Wrap().Build(), engine: engine);

        // Disable threshold by setting it to null
        await runner.Host.Compression(CompressedContent.Default().MinimumSize(null)).StartAsync();

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, br, zstd");

        using var response = await runner.GetResponseAsync(request);

        // Content should be compressed even though it's very small, as threshold is disabled
        Assert.IsNotEmpty(response.Content.Headers.ContentEncoding);
        Assert.AreEqual("zstd", response.Content.Headers.ContentEncoding.First());

        await runner.DisposeAsync();
    }

    /// <summary>
    /// Helper class to create content with unknown length (null)
    /// </summary>
    private class UnknownLengthContent : IResponseContent
    {
        private readonly IResponseContent _wrapped;

        public UnknownLengthContent(IResponseContent wrapped)
        {
            _wrapped = wrapped;
        }

        public ulong? Length => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => _wrapped.CalculateChecksumAsync();

        public ValueTask WriteAsync(Stream target, uint bufferSize) => _wrapped.WriteAsync(target, bufferSize);

    }

    private class CustomAlgorithm : ICompressionAlgorithm
    {

        public string Name => "custom";

        public Priority Priority => Priority.High;

        public IResponseContent Compress(IResponseContent content, CompressionLevel level) => content;

        public Stream Decompress(Stream stream) => throw new NotImplementedException();

    }

    /// <summary>
    /// Helper method to create large content that will be compressed (above default threshold of 256 bytes).
    /// </summary>
    private static LayoutBuilder CreateLargeContentHandler()
    {
        var content = Content.From(Resource.FromString(CreateLargeString(500)).Type(ContentType.TextHtml));

        return Layout.Create()
                     .Index(content);
    }

    /// <summary>
    /// Helper method to create a large string of specified size.
    /// </summary>
    private static string CreateLargeString(int sizeInBytes)
    {
        return new string('A', sizeInBytes);
    }

}
