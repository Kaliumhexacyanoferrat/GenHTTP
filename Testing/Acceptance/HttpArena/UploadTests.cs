using System.Net;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "upload" scenario.
/// Raw requests: upload.raw (POST /upload with 20MB body), upload-small.raw (1MB),
/// upload-500k.raw, upload-2m.raw, upload-10m.raw, upload-20m.raw.
/// </summary>
[TestClass]
public sealed class UploadTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestUploadSmall(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var data = new byte[1024];
        Random.Shared.NextBytes(data);

        var request = host.GetRequest("/upload");
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent(data);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("1024", body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpload500k(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var data = new byte[500 * 1024];
        Random.Shared.NextBytes(data);

        var request = host.GetRequest("/upload");
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent(data);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual((500 * 1024).ToString(), body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpload2m(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var data = new byte[2 * 1024 * 1024];
        Random.Shared.NextBytes(data);

        var request = host.GetRequest("/upload");
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent(data);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual((2 * 1024 * 1024).ToString(), body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUploadEmpty(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/upload");
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent([]);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("0", body.Trim());
    }
}
