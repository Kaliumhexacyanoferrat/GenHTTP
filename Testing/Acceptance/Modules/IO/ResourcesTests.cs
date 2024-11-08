using System.Net;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class ResourcesTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFileDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is text!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSubdirectoryFileDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/Subdirectory/AnotherFile.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is another text!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoFileDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/nah.txt");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoSubdirectoryFileDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/nah/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRootDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly("Resources")), engine: engine);

        using var response = await runner.GetResponseAsync("/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is text!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDirectory(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/nah/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNonExistingDirectory(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()), engine: engine);

        using var response = await runner.GetResponseAsync("/Resources/nah/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

}
