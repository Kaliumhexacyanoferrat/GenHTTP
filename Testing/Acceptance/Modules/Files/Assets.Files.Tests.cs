using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.Files.Multi;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Files;

[TestClass]
public sealed class AssetsFilesTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRegular(ServerEngine engine)
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
    public async Task TestSubFile(ServerEngine engine)
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
    public async Task TestSubFileCompressed(ServerEngine engine)
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
            h.AllowPrecompressed([new BrotliAlgorithm()], '.');
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSubFileGzip(ServerEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            var request = host.GetRequest("/SubDir/subfile.txt");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            // Served from the ".gz" sibling, but announced with the "gzip" Content-Encoding.
            Assert.AreEqual("This is subgzip", await response.GetContentAsync());
            Assert.AreEqual("gzip", response.GetContentHeader("Content-Encoding"));
            Assert.AreEqual("Accept-Encoding", response.GetHeader("Vary"));
        }, Adjustments);

        return;

        void Adjustments(FileAssetsBuilder h)
        {
            h.AllowPrecompressed([new BrotliAlgorithm(), new GzipAlgorithm()], '.');
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDirectory(ServerEngine engine)
    {
        await RunAsync(engine, async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        });
    }

    [TestMethod]
    public void TestChaining() => Chain.Works(Assets.From("./"));

    private async ValueTask RunAsync(ServerEngine engine, Func<TestHost, ValueTask> logic, Action<FileAssetsBuilder>? customizations = null)
    {
        var dir = Directory.CreateTempSubdirectory();

        var rootFile = Path.Combine(dir.FullName, "file.txt");

        await File.WriteAllTextAsync(rootFile, "This is root");

        var subDir = dir.CreateSubdirectory("SubDir");

        var subFile = Path.Combine(subDir.FullName, "subfile.txt");

        await File.WriteAllTextAsync(subFile, "This is sub");

        var subFileCompressed = Path.Combine(subDir.FullName, "subfile.txt.br");

        await File.WriteAllTextAsync(subFileCompressed, "This is subcompressed");

        // gzip's sibling uses the ".gz" extension (its FileExtension), not the ".gzip" encoding name.
        var subFileGzip = Path.Combine(subDir.FullName, "subfile.txt.gz");

        await File.WriteAllTextAsync(subFileGzip, "This is subgzip");

        var assets = Assets.From(dir);

        customizations?.Invoke(assets);

        Assert.IsInstanceOfType<FileAssetsHandler>(assets.Build());

        await using var host = await TestHost.RunAsync(assets, engine: engine);

        await logic(host);
    }

}
