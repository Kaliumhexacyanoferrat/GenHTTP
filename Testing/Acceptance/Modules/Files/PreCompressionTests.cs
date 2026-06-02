using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Files;

[TestClass]
public class PreCompressionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRegular(TestEngine engine)
    {
        await using var runner = await RunAsync(engine);

        var request = runner.GetRequest("/Resources/LargeTemplate.html");

        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("br", response.Content.Headers.ContentEncoding.First());

        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.ToString());

        var content = await response.GetContentAsync();

        Assert.DoesNotContain("precompressed", content); // not automatically decompressed
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNonSupportedAlgorithm(TestEngine engine)
    {
        await using var runner = await RunAsync(engine);

        var request = runner.GetRequest("/Resources/LargeTemplate.html");

        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsEmpty(response.Content.Headers.ContentEncoding);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoCompressionRequested(TestEngine engine)
    {
        await using var runner = await RunAsync(engine);

        using var response = await runner.GetResponseAsync("/Resources/LargeTemplate.html");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsEmpty(response.Content.Headers.ContentEncoding);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFolderNotFound(TestEngine engine)
    {
        await using var runner = await RunAsync(engine);

        var request = runner.GetRequest("/Resources/SubDirectory/");

        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public void TestChaining()
    {
        var handler = Assets.From(ResourceTree.FromAssembly())
                            .AllowPrecompressed(new BrotliAlgorithm());

        Chain.Works(handler);
    }

    private static async ValueTask<TestHost> RunAsync(TestEngine engine)
    {
        var handler = Assets.From(ResourceTree.FromAssembly())
                            .AllowPrecompressed([new BrotliAlgorithm()], '+');

        return await TestHost.RunAsync(handler, defaults: false, engine: engine);
    }

}
