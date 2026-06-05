using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "json" and "json-comp" scenarios.
/// Raw requests: json-get.raw (GET /json/50?m=1), json-get-gzip.raw (with Accept-Encoding: gzip, br),
/// json-1.raw through json-50.raw with varying counts and multipliers.
/// </summary>
[TestClass]
public sealed class JsonTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestJson50Items(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/json/50?m=1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(50, result.GetProperty("count").GetInt32());
        Assert.AreEqual(50, result.GetProperty("items").GetArrayLength());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJson1Item(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/json/1?m=3");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(1, result.GetProperty("count").GetInt32());
        Assert.AreEqual(1, result.GetProperty("items").GetArrayLength());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonTotalIsComputedCorrectly(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/json/1?m=2");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        var item = result.GetProperty("items")[0];
        var price = item.GetProperty("price").GetInt64();
        var quantity = item.GetProperty("quantity").GetInt64();
        var total = item.GetProperty("total").GetInt64();

        Assert.AreEqual(price * quantity * 2, total);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonMultiplierVariants(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        foreach (var count in new[] { 5, 10, 15, 25, 40 })
        {
            using var response = await host.GetResponseAsync($"/json/{count}?m=1");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var body = await response.GetContentAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            Assert.AreEqual(count, result.GetProperty("count").GetInt32(), $"Expected {count} items");
        }
    }

    /// <summary>
    /// Corresponds to json-comp scenario — same endpoint with Accept-Encoding: gzip, br.
    /// Verifies that the server responds with 200 and applies compression.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonWithGzipAcceptEncoding(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var request = host.GetRequest("/json/50?m=1");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 0.8));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br", 1.0));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsTrue(response.Content.Headers.ContentEncoding.Any(),
            "Server should apply compression when client advertises Accept-Encoding");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonGzipVariants(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        foreach (var count in new[] { 1, 5, 10, 15, 25, 40, 50 })
        {
            var request = host.GetRequest($"/json/{count}?m=1");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Any(),
                $"Server should compress response for count={count}");
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonClampedCount(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/json/999?m=1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.IsTrue(result.GetProperty("count").GetInt32() <= 50, "Count should be clamped to dataset size");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonZeroCount(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/json/0?m=1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(0, result.GetProperty("count").GetInt32());
    }
}
