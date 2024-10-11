using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class SinglePageTests
{

    [TestMethod]
    public async Task TestIndex()
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "index.html"), "This is the index!");

        using var runner = TestHost.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

        using var index = await runner.GetResponseAsync("/");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));

        var content = await index.GetContentAsync();

        Assert.AreEqual("This is the index!", content);
    }

    [TestMethod]
    public async Task TestIndexServedWithRouting()
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "index.html"), "This is the index!");

        var spa = SinglePageApplication.From(ResourceTree.FromDirectory(root))
                                       .ServerSideRouting();

        using var runner = TestHost.Run(spa);

        using var index = await runner.GetResponseAsync("/some-route/");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));
    }

    [TestMethod]
    public async Task TestNoIndex()
    {
        using var runner = TestHost.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

        using var index = await runner.GetResponseAsync("/");

        await index.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task TestFile()
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "some.txt"), "This is some text file :)");

        using var runner = TestHost.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

        using var index = await runner.GetResponseAsync("/some.txt");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/plain", index.GetContentHeader("Content-Type"));

        var content = await index.GetContentAsync();

        Assert.AreEqual("This is some text file :)", content);
    }

    [TestMethod]
    public async Task TestNoFile()
    {
        using var runner = TestHost.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

        using var index = await runner.GetResponseAsync("/nope.txt");

        await index.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    private static string CreateRoot()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        Directory.CreateDirectory(tempDirectory);

        return tempDirectory;
    }
}
