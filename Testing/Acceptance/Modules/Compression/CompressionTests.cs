using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        await using var runner = await TestHost.RunAsync(Layout.Create().Build(), engine: engine);

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
            await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

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

        Assert.IsFalse(response.Content.Headers.ContentEncoding.Count != 0);
    }

    /// <summary>
    /// As a developer, I want to be able to add custom compression algorithms.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomCompression(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

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

        Assert.IsFalse(response.Content.Headers.ContentEncoding.Count != 0);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVariyHeaderAdded(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("Accept-Encoding", response.GetHeader("Vary"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVariyHeaderExtendedAdded(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Vary", "Host")
                    .Content(Resource.FromString("Hello World").Build())
                    .Type(ContentType.TextHtml)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip");

        using var response = await runner.GetResponseAsync(request);

        Assert.IsTrue(response.Headers.Vary.Contains("Host"));
        Assert.IsTrue(response.Headers.Vary.Contains("Accept-Encoding"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentType(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Content(Resource.FromString("Hello World").Build())
                    .Type("application/json; charset=utf-8")
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("br", response.Content.Headers.ContentEncoding.First());
    }

    private class CustomAlgorithm : ICompressionAlgorithm
    {

        public string Name => "custom";

        public Priority Priority => Priority.High;

        public IResponseContent Compress(IResponseContent content, CompressionLevel level) => content;
    }

}
