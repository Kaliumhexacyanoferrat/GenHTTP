using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

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

    private static async ValueTask<TestHost> RunAsync(TestEngine engine)
    {
        var handler = PreCompressedResources.From(ResourceTree.FromAssembly())
                                            .Add(new BrotliAlgorithm());

        return await TestHost.RunAsync(handler, defaults: false, engine: engine);
    }

}
