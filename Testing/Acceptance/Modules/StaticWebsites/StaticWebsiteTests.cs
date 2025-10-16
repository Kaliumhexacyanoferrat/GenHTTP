using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.StaticWebsites;

[TestClass]
public sealed class StaticWebsiteTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestWithIndex(TestEngine engine)
    {
        var tree = VirtualTree.Create()
                              .Add("index.html", Resource.FromString("Index 1"))
                              .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")));

        await using var runner = await TestHost.RunAsync(StaticWebsite.From(tree), engine: engine);

        using var indexResponse = await runner.GetResponseAsync();
        Assert.AreEqual("Index 1", await indexResponse.GetContentAsync());

        using var subIndexResponse = await runner.GetResponseAsync("/sub/");
        Assert.AreEqual("Index 2", await subIndexResponse.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoIndex(TestEngine engine)
    {
        var tree = VirtualTree.Create()
                              .Add("sub", VirtualTree.Create());

        await using var runner = await TestHost.RunAsync(StaticWebsite.From(tree), engine: engine);

        using var indexResponse = await runner.GetResponseAsync();
        await indexResponse.AssertStatusAsync(HttpStatusCode.NotFound);

        using var subIndexResponse = await runner.GetResponseAsync("/sub/");
        await subIndexResponse.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public void TestConcernChaining()
    {
        var site = StaticWebsite.From(VirtualTree.Create());

        Chain.Works(site);
    }

}
