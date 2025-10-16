using System.Net;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public class WildcardTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRouting(TestEngine engine)
    {
        var tree = ResourceTree.FromAssembly("Resources");

        var resources = Resources.From(tree);

        var app = Inline.Create()
                        .Get("/tenant/:tenantID/", (int tenantID) =>
                        {
                             Assert.IsGreaterThan(0, tenantID);
                             return resources;
                        });

        await using var host = await TestHost.RunAsync(app);

        using var root = await host.GetResponseAsync("/tenant/1/File.txt");

        await root.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is text!", await root.GetContentAsync());

        using var sub = await host.GetResponseAsync("/tenant/42/Subdirectory/AnotherFile.txt");

        await sub.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is another text!", await sub.GetContentAsync());
    }

}
