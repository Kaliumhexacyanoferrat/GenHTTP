using System.Net;
using GenHTTP.Modules.Archives;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;

namespace GenHTTP.Testing.Acceptance.Modules.Archives;

[TestClass]
public class IntegrationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticFiles(TestEngine engine)
    {
        var tree = ArchiveTree.From(Resource.FromAssembly("Archive.zip"));

        var app = StaticWebsite.From(tree);

        await using var runner = await TestHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync("/SubDir/SubFile.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("2", await response.GetContentAsync());
    }

}
