using System.Net;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class ResourcesTests
{

    [TestMethod]
    public async Task TestFileDownload()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is text!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestSubdirectoryFileDownload()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/Subdirectory/AnotherFile.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is another text!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestNoFileDownload()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/nah.txt");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task TestNoSubdirectoryFileDownload()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/nah/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task TestRootDownload()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly("Resources")));

        using var response = await runner.GetResponseAsync("/File.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("This is text!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestDirectory()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/nah/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task TestNonExistingDirectory()
    {
        await using var runner = await TestHost.RunAsync(Resources.From(ResourceTree.FromAssembly()));

        using var response = await runner.GetResponseAsync("/Resources/nah/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }
}
