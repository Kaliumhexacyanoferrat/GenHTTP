#if NET11_0_OR_GREATER

using System.Net;
using System.Net.Http.Headers;

using GenHTTP.Testing.Acceptance.Engine;
using GenHTTP.Testing.Acceptance.Utilities;

using IoxideFilesModule = GenHTTP.Modules.IoxideFiles.IoxideFiles;

namespace GenHTTP.Testing.Acceptance.Modules.IoxideFiles;

[TestClass]
public sealed class IoxideFilesTests
{

    [TestMethod]
    public async Task TestRegular()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            using var response = await host.GetResponseAsync("/file.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is root", await response.GetContentAsync());
            Assert.AreEqual("text/plain", response.Content.Headers.ContentType?.MediaType);
        });
    }

    [TestMethod]
    public async Task TestSubFile()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/subfile.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is sub", await response.GetContentAsync());
        });
    }

    [TestMethod]
    public async Task TestNotFound()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            using var response = await host.GetResponseAsync("/does-not-exist.txt");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        });
    }

    [TestMethod]
    public async Task TestDirectory()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        });
    }

    [TestMethod]
    public async Task TestMethodNotAllowed()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            var request = host.GetRequest("/file.txt", HttpMethod.Post);

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);

            // "Allow" is comma-separated and gets split into tokens by HttpHeaders, so
            // GetContentHeader's FirstOrDefault() would only see "GET" - check the full set instead.
            CollectionAssert.AreEquivalent(new[] { "GET", "HEAD" }, response.Content.Headers.Allow.ToList());
        });
    }

    [TestMethod]
    public async Task TestHead()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            var request = host.GetRequest("/file.txt", HttpMethod.Head);

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("", await response.GetContentAsync());
            Assert.AreEqual((long)"This is root".Length, response.Content.Headers.ContentLength);
        });
    }

    [TestMethod]
    public async Task TestPrecompressedBrotli()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            var request = host.GetRequest("/SubDir/subfile.txt");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is sub, compressed with brotli", await response.GetContentAsync());
            Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
            Assert.AreEqual("Accept-Encoding", response.GetHeader("Vary"));
        });
    }

    [TestMethod]
    public async Task TestPrecompressedGzip()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            var request = host.GetRequest("/SubDir/subfile.txt");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is sub, compressed with gzip", await response.GetContentAsync());
            Assert.AreEqual("gzip", response.GetContentHeader("Content-Encoding"));
        });
    }

    [TestMethod]
    public async Task TestPrecompressedPrefersBrotliOverGzip()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            var request = host.GetRequest("/SubDir/subfile.txt");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
        });
    }

    [TestMethod]
    public async Task TestNoAcceptEncodingServesIdentity()
    {
        if (!Engines.IoxideEnabled()) return;

        await RunAsync(async host =>
        {
            using var response = await host.GetResponseAsync("/SubDir/subfile.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("This is sub", await response.GetContentAsync());
            Assert.IsNull(response.GetContentHeader("Content-Encoding"));
        });
    }

    [TestMethod]
    public async Task TestLargeFileIsServedFromDisk()
    {
        if (!Engines.IoxideEnabled()) return;

        var dir = Directory.CreateTempSubdirectory();

        // AssetCache.DefaultMaxCachedFileBytes is 256 KB - go over it so the asset is read off the ring
        // instead of being served from the baked in-memory response.
        var content = string.Concat(Enumerable.Repeat("0123456789", 30_000));

        await File.WriteAllTextAsync(Path.Combine(dir.FullName, "large.txt"), content);

        var handler = IoxideFilesModule.From(dir.FullName);

        await using var host = await TestHost.RunAsync(handler, engine: TestEngine.Ioxide);

        for (var i = 0; i < 3; i++)
        {
            using var response = await host.GetResponseAsync("/large.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual(content, await response.GetContentAsync());
        }
    }

    [TestMethod]
    public async Task TestChangedFileServesUpdatedContent()
    {
        if (!Engines.IoxideEnabled()) return;

        var dir = Directory.CreateTempSubdirectory();

        var file = Path.Combine(dir.FullName, "file.txt");

        await File.WriteAllTextAsync(file, "This is root");

        var handler = IoxideFilesModule.From(dir.FullName);

        // Edited after the cache snapshot was taken, with a different length so IsFresh's size check fails.
        await File.WriteAllTextAsync(file, "This is the updated content");

        await using var host = await TestHost.RunAsync(handler, engine: TestEngine.Ioxide);

        using var response = await host.GetResponseAsync("/file.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("This is the updated content", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestChaining()
    {
        if (!Engines.IoxideEnabled()) return;

        var dir = Directory.CreateTempSubdirectory();

        Chain.Works(IoxideFilesModule.From(dir.FullName));
    }

    private static async ValueTask RunAsync(Func<TestHost, ValueTask> logic)
    {
        var dir = Directory.CreateTempSubdirectory();

        var rootFile = Path.Combine(dir.FullName, "file.txt");

        await File.WriteAllTextAsync(rootFile, "This is root");

        var subDir = dir.CreateSubdirectory("SubDir");

        var subFile = Path.Combine(subDir.FullName, "subfile.txt");

        await File.WriteAllTextAsync(subFile, "This is sub");

        var subFileBrotli = Path.Combine(subDir.FullName, "subfile.txt.br");

        await File.WriteAllTextAsync(subFileBrotli, "This is sub, compressed with brotli");

        var subFileGzip = Path.Combine(subDir.FullName, "subfile.txt.gz");

        await File.WriteAllTextAsync(subFileGzip, "This is sub, compressed with gzip");

        var handler = IoxideFilesModule.From(dir.FullName);

        await using var host = await TestHost.RunAsync(handler, engine: TestEngine.Ioxide);

        await logic(host);
    }

}

#endif
