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
    [MultiEngineTest]
    public async Task TestIndex(TestEngine engine)
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "index.html"), "This is the index!");

        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(root)), engine: engine);

        using var index = await runner.GetResponseAsync("/");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));

        var content = await index.GetContentAsync();

        Assert.AreEqual("This is the index!", content);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIndexServedWithRouting(TestEngine engine)
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "index.html"), "This is the index!");

        var spa = SinglePageApplication.From(ResourceTree.FromDirectory(root))
                                       .ServerSideRouting();

        await using var runner = await TestHost.RunAsync(spa, engine: engine);

        using var index = await runner.GetResponseAsync("/some-route/");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoIndex(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        using var index = await runner.GetResponseAsync("/");

        await index.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFile(TestEngine engine)
    {
        var root = CreateRoot();

        await FileUtil.WriteTextAsync(Path.Combine(root, "some.txt"), "This is some text file :)");

        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(root)), engine: engine);

        using var index = await runner.GetResponseAsync("/some.txt");

        await index.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("text/plain", index.GetContentHeader("Content-Type"));

        var content = await index.GetContentAsync();

        Assert.AreEqual("This is some text file :)", content);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoFile(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

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
