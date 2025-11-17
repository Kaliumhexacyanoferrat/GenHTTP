using System.IO.Compression;
using System.Net;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ServerCaching;

namespace GenHTTP.Testing.Acceptance.Modules.ServerCaching;

[TestClass]
public class PrecompressedContentTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentCanBePreCompressed(TestEngine engine)
    {
        var content = Resources.From(ResourceTree.FromAssembly("Resources"))
                               .Add(CompressedContent.Default().Level(CompressionLevel.Optimal))
                               .Add(ServerCache.Memory());

        await using var runner = await TestHost.RunAsync(content, false, engine: engine);

        var request = runner.GetRequest("/LargeTemplate.html");

        request.Headers.Add("Accept-Encoding", "br");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
    }

}
