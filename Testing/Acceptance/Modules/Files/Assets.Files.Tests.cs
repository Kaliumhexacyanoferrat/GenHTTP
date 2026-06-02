using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.Files.Multi;

namespace GenHTTP.Testing.Acceptance.Modules.Files;

[TestClass]
public sealed class AssetsFilesTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRegular(TestEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            using var response = await host.GetResponseAsync("/file.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is root", await response.GetContentAsync());
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSubFile(TestEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/subfile.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is sub", await response.GetContentAsync());
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSubFileCompressed(TestEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            var request = host.GetRequest("/SubDir/subfile.txt");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is subcompressed", await response.GetContentAsync());
            Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
        }, Adjustments);

        return;

        void Adjustments(FileAssetsBuilder h)
        {
            h.AllowPrecompressed(new BrotliAlgorithm());
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDirectory(TestEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        });
    }

    private async ValueTask RunAsync(TestEngine engine, Func<TestHost, ValueTask> logic, Action<FileAssetsBuilder>? customizations = null)
    {
        var dir = Directory.CreateTempSubdirectory();

        var rootFile = Path.Combine(dir.FullName, "file.txt");

        await File.WriteAllTextAsync(rootFile, "This is root");

        var subDir = dir.CreateSubdirectory("SubDir");

        var subFile = Path.Combine(subDir.FullName, "subfile.txt");

        await File.WriteAllTextAsync(subFile, "This is sub");

        var subFileCompressed = Path.Combine(subDir.FullName, "subfile.txt.br");

        await File.WriteAllTextAsync(subFileCompressed, "This is subcompressed");

        var assets = Assets.From(dir);

        customizations?.Invoke(assets);

        using var handler = (FileAssetsHandler)assets.Build();

        await using var host = await TestHost.RunAsync(assets, engine: engine);

        await logic(host);
    }

}
