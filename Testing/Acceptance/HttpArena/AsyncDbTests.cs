using System.Net;
using System.Text.Json;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "async-db" scenario.
/// Raw requests: async-db-get.raw (GET /async-db?min=10&amp;max=50&amp;limit=50),
/// async-db-5.raw through async-db-50.raw with varying limits.
/// Uses mocked JSON data instead of a real PostgreSQL database.
/// </summary>
[TestClass]
public sealed class AsyncDbTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbDefault(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/async-db?min=10&max=50&limit=50");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.IsTrue(result.GetProperty("count").GetInt32() >= 0);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbLimitRespected(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/async-db?min=10&max=50&limit=5");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.IsTrue(result.GetProperty("count").GetInt32() <= 5, "Result should respect limit of 5");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbLimitVariants(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        foreach (var limit in new[] { 5, 10, 20, 35, 50 })
        {
            using var response = await host.GetResponseAsync($"/async-db?min=10&max=50&limit={limit}");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var body = await response.GetContentAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            Assert.IsTrue(result.GetProperty("count").GetInt32() <= limit, $"Limit {limit} not respected");
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbPriceRangeFiltered(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/async-db?min=100&max=200&limit=50");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        var items = result.GetProperty("items");

        foreach (var item in items.EnumerateArray())
        {
            var price = item.GetProperty("price").GetInt32();
            Assert.IsTrue(price >= 100 && price <= 200, $"Price {price} is outside [100, 200] range");
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbNarrowRange(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/async-db?min=1000&max=1000&limit=50");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(0, result.GetProperty("count").GetInt32(), "No items should match price=1000");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncDbItemShape(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/async-db?min=1&max=9999&limit=1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        if (result.GetProperty("count").GetInt32() > 0)
        {
            var item = result.GetProperty("items")[0];

            Assert.IsTrue(item.TryGetProperty("id", out _), "Missing 'id'");
            Assert.IsTrue(item.TryGetProperty("name", out _), "Missing 'name'");
            Assert.IsTrue(item.TryGetProperty("category", out _), "Missing 'category'");
            Assert.IsTrue(item.TryGetProperty("price", out _), "Missing 'price'");
            Assert.IsTrue(item.TryGetProperty("quantity", out _), "Missing 'quantity'");
            Assert.IsTrue(item.TryGetProperty("active", out _), "Missing 'active'");
            Assert.IsTrue(item.TryGetProperty("tags", out _), "Missing 'tags'");
            Assert.IsTrue(item.TryGetProperty("rating", out _), "Missing 'rating'");
        }
    }
}
