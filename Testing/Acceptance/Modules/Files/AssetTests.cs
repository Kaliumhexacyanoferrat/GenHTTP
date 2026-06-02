using System.Net;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Files;

[TestClass]
public sealed class AssetTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Asset.From(Resource.FromAssembly("File.txt")), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("This is text!", await response.GetContentAsync());
        Assert.AreEqual("text/plain", response.GetContentHeader("Content-Type"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDownloadDoesNotAcceptRouting(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Add("file.txt", Asset.From(Resource.FromAssembly("File.txt")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/file.txt/blubb");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task DownloadsCannotBeModified(TestEngine engine)
    {
        var download = Asset.From(Resource.FromAssembly("File.txt"));

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Put;
        request.Content = new StringContent("Hello World!", Encoding.UTF8, "text/plain");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);

        Assert.AreEqual("GET", response.GetContentHeader("Allow"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFileName(TestEngine engine)
    {
        var download = Asset.From(Resource.FromAssembly("File.txt"))
                            .AsDownload("myfile.txt");

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoFileName(TestEngine engine)
    {
        var download = Asset.From(Resource.FromAssembly("File.txt"))
                            .AsDownload();

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFileNameFromResource(TestEngine engine)
    {
        var download = Asset.From(Resource.FromAssembly("File.txt").Name("myfile.txt"))
                            .AsDownload();

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTypeCanBeSet(TestEngine engine)
    {
        var download = Asset.From(Resource.FromString("This;is;CSV"))
                            .Type(ContentType.TextCsv);

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("text/csv", response.GetContentHeader("Content-Type"));
    }

}
