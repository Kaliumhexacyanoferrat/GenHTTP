using System.Net;
using System.Text;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "baseline", "pipelined", and "limited-conn" scenarios.
/// Raw requests: get.raw (GET /baseline11?a=13&amp;b=42), pipeline.raw (GET /pipeline)
/// </summary>
[TestClass]
public sealed class BaselineTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestBaselineSum(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/baseline11?a=13&b=42");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("55", body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBaselineLargeNumbers(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/baseline11?a=1000&b=2000");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("3000", body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBaselineZero(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/baseline11?a=0&b=0");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("0", body.Trim());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBaselinePost(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/baseline11?a=10&b=20");
        request.Method = HttpMethod.Post;
        request.Content = new StringContent("5", Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        Assert.AreEqual("35", body.Trim());
    }

    /// <summary>
    /// Corresponds to the "pipelined" scenario — GET /pipeline returns "ok".
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestPipeline(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/pipeline");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("ok", await response.GetContentAsync());
    }
}
