using System.IO.Compression;
using System.Net;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ServerCaching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ServerCaching;

[TestClass]
public class PrecompressedContentTest
{

    [TestMethod]
    public async Task TestContentCanBePreCompressed()
    {
        var content = Resources.From(ResourceTree.FromAssembly("Resources"))
                               .Add(CompressedContent.Default().Level(CompressionLevel.Optimal))
                               .Add(ServerCache.Memory());

        using var runner = TestHost.Run(content, false);

        var request = runner.GetRequest("/Template.html");

        request.Headers.Add("Accept-Encoding", "br");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
    }
}
