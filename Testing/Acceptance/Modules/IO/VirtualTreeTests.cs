using System.Net;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class VirtualTreeTests
{

    [TestMethod]
    public async Task TestNestedTree()
    {
        var tree = ResourceTree.FromAssembly("Resources");

        var virt = VirtualTree.Create()
                              .Add("r", tree)
                              .Build();

        var (node, file) = await virt.Find(GetTarget("/r/File.txt"));

        Assert.IsNotNull(node);
        Assert.IsNotNull(file);

        Assert.IsTrue((node as IResourceNode)?.Parent == virt);

        Assert.IsNotNull(virt.Modified);
    }

    [TestMethod]
    public async Task TestResource()
    {
        var virt = VirtualTree.Create()
                              .Add("res.txt", Resource.FromString("Blubb"))
                              .Build();

        var (node, file) = await virt.Find(GetTarget("/res.txt"));

        Assert.IsNotNull(node);
        Assert.IsNotNull(file);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUsage(TestEngine engine)
    {
        var tree = ResourceTree.FromAssembly("Resources");

        var virt = VirtualTree.Create()
                              .Add("r", tree)
                              .Add("res.txt", Resource.FromString("Blubb"));

        var handler = Listing.From(virt);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    private static RoutingTarget GetTarget(string path) => new(WebPath.FromString(path));
}
