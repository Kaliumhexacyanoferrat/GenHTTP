using System.Net;
using System.Net.Http.Headers;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "static" scenario.
/// Raw requests: static-header.html.raw, static-footer.html.raw, static-app.js.raw,
/// static-app.js-gzip.raw, static-components.css.raw, etc.
/// Serves pre-compressed files from the HttpArena data/static directory.
/// </summary>
[TestClass]
public sealed class StaticTests
{
    private static bool StaticFilesAvailable =>
        Directory.Exists(Path.Combine(HttpArenaProject.DataPath, "static"));

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticHeaderHtml(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/header.html");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
        Assert.IsTrue(contentType.Contains("text/html"), $"Expected text/html, got {contentType}");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticHeaderHtmlBrotli(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/static/header.html");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br", 1.0));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 0.8));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("br", response.Content.Headers.ContentEncoding.FirstOrDefault(),
            "Brotli pre-compressed file should be served when br is accepted");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticFooterHtml(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/footer.html");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticAppJs(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/app.js");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
        Assert.IsTrue(contentType.Contains("javascript") || contentType.Contains("text"),
            $"Expected JS content type, got {contentType}");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticAppJsGzip(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/static/app.js");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br", 1.0));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 0.8));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticComponentsCss(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/components.css");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
        Assert.IsTrue(contentType.Contains("css") || contentType.Contains("text"),
            $"Expected CSS content type, got {contentType}");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticComponentsCssBrotli(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/static/components.css");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br", 1.0));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("br", response.Content.Headers.ContentEncoding.FirstOrDefault());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticHeroWebp(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/hero.webp");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticBoldWoff2(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/bold.woff2");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticLogoSvg(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/logo.svg");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticManifestJson(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/manifest.json");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticNotFound(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/static/nonexistent.file");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticAllFiles(TestEngine engine)
    {
        if (!StaticFilesAvailable) Assert.Inconclusive("HttpArena static files not available");

        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var files = new[]
        {
            "analytics.js",
            "helpers.js",
            "router.js",
            "vendor.js",
            "layout.css",
            "reset.css",
            "theme.css",
            "utilities.css",
            "icon-sprite.svg",
            "regular.woff2",
            "thumb1.webp",
            "thumb2.webp"
        };

        foreach (var file in files)
        {
            using var response = await host.GetResponseAsync($"/static/{file}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"File /static/{file} should return 200");
        }
    }
}
