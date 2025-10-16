using System.Net;
using System.Text;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class DownloadTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDownload(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Download.From(Resource.FromAssembly("File.txt")), engine: engine);

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
                           .Add("file.txt", Download.From(Resource.FromAssembly("File.txt")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/file.txt/blubb");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task DownloadsCannotBeModified(TestEngine engine)
    {
        var download = Download.From(Resource.FromAssembly("File.txt"));

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
        var download = Download.From(Resource.FromAssembly("File.txt"))
                               .FileName("myfile.txt");

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoFileName(TestEngine engine)
    {
        var download = Download.From(Resource.FromAssembly("File.txt"));

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFileNameFromResource(TestEngine engine)
    {
        var download = Download.From(Resource.FromAssembly("File.txt").Name("myfile.txt"));

        await using var runner = await TestHost.RunAsync(download, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
    }
}
