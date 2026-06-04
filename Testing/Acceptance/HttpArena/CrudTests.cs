using System.Net;
using System.Text;
using System.Text.Json;

namespace GenHTTP.Testing.Acceptance.HttpArena;

/// <summary>
/// Tests for HttpArena "crud" scenario.
/// Raw requests: crud-get-1.raw through crud-get-15.raw (GET /crud/items/{id}),
/// crud-create-1.raw (POST /crud/items), crud-update-1.raw (PUT /crud/items/{id}),
/// crud-list-rand-1.raw (GET /crud/items?category=office&amp;page=N&amp;limit=10).
/// Uses mocked in-memory data instead of a real PostgreSQL database.
/// </summary>
[TestClass]
public sealed class CrudTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetExistingItem(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/crud/items/1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var item = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(1, item.GetProperty("id").GetInt32());
        Assert.AreEqual("MISS", response.Headers.GetValues("X-Cache").FirstOrDefault());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetItemCacheHit(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var first = await host.GetResponseAsync("/crud/items/2");
        await first.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("MISS", first.Headers.GetValues("X-Cache").FirstOrDefault());

        using var second = await host.GetResponseAsync("/crud/items/2");
        await second.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("HIT", second.Headers.GetValues("X-Cache").FirstOrDefault());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetMultipleItems(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        foreach (var id in Enumerable.Range(1, 15))
        {
            using var response = await host.GetResponseAsync($"/crud/items/{id}");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var body = await response.GetContentAsync();
            var item = JsonSerializer.Deserialize<JsonElement>(body);

            Assert.AreEqual(id, item.GetProperty("id").GetInt32());
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetNonExistentItem(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/crud/items/99999");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCreateItem(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var payload = JsonSerializer.Serialize(new
        {
            id = 100001,
            name = "New Product",
            category = "test",
            price = 150,
            quantity = 30
        });

        var request = host.GetRequest("/crud/items");
        request.Method = HttpMethod.Post;
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.Created);

        var body = await response.GetContentAsync();
        var item = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(100001, item.GetProperty("id").GetInt32());
        Assert.AreEqual(150, item.GetProperty("price").GetInt32());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpdateItem(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var payload = JsonSerializer.Serialize(new
        {
            name = "Updated Product",
            category = "test",
            price = 200,
            quantity = 25
        });

        var request = host.GetRequest("/crud/items/1");
        request.Method = HttpMethod.Put;
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var item = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(200, item.GetProperty("price").GetInt32());
        Assert.AreEqual(25, item.GetProperty("quantity").GetInt32());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpdateNonExistentItem(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var payload = JsonSerializer.Serialize(new { name = "Ghost", price = 1, quantity = 1 });

        var request = host.GetRequest("/crud/items/99999");
        request.Method = HttpMethod.Put;
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestListByCategory(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        using var response = await host.GetResponseAsync("/crud/items?category=electronics&page=1&limit=10");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var body = await response.GetContentAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(1, result.GetProperty("page").GetInt32());
        Assert.AreEqual(10, result.GetProperty("limit").GetInt32());

        var items = result.GetProperty("items");

        foreach (var item in items.EnumerateArray())
        {
            Assert.AreEqual("electronics", item.GetProperty("category").GetString());
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestListPagination(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        foreach (var page in Enumerable.Range(1, 10))
        {
            using var response = await host.GetResponseAsync($"/crud/items?category=electronics&page={page}&limit=10");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var body = await response.GetContentAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            Assert.AreEqual(page, result.GetProperty("page").GetInt32());
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCreateThenGet(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        var newId = 200001;
        var payload = JsonSerializer.Serialize(new { id = newId, name = "Bench Item", category = "bench", price = 99, quantity = 5 });

        var createRequest = host.GetRequest("/crud/items");
        createRequest.Method = HttpMethod.Post;
        createRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var createResponse = await host.GetResponseAsync(createRequest);
        await createResponse.AssertStatusAsync(HttpStatusCode.Created);

        using var getResponse = await host.GetResponseAsync($"/crud/items/{newId}");
        await getResponse.AssertStatusAsync(HttpStatusCode.OK);

        var body = await getResponse.GetContentAsync();
        var item = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.AreEqual(newId, item.GetProperty("id").GetInt32());
        Assert.AreEqual("Bench Item", item.GetProperty("name").GetString());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpdateInvalidatesCacheHit(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(HttpArenaProject.Create(), engine: engine);

        // PUT first to ensure the cache entry is cleared regardless of prior test state
        var clearPayload = JsonSerializer.Serialize(new { name = "Pre-clear", price = 50, quantity = 1 });
        var clearRequest = host.GetRequest("/crud/items/5");
        clearRequest.Method = HttpMethod.Put;
        clearRequest.Content = new StringContent(clearPayload, Encoding.UTF8, "application/json");

        using var clearResponse = await host.GetResponseAsync(clearRequest);
        await clearResponse.AssertStatusAsync(HttpStatusCode.OK);

        // First GET after PUT: must be MISS (cache was cleared by the PUT above)
        using var miss = await host.GetResponseAsync("/crud/items/5");
        await miss.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("MISS", miss.Headers.GetValues("X-Cache").FirstOrDefault());

        // Second GET: must be HIT (loaded from cache)
        using var hit = await host.GetResponseAsync("/crud/items/5");
        await hit.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("HIT", hit.Headers.GetValues("X-Cache").FirstOrDefault());

        // PUT again — must invalidate cache
        var payload = JsonSerializer.Serialize(new { name = "Changed", price = 1, quantity = 1 });
        var updateRequest = host.GetRequest("/crud/items/5");
        updateRequest.Method = HttpMethod.Put;
        updateRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var updateResponse = await host.GetResponseAsync(updateRequest);
        await updateResponse.AssertStatusAsync(HttpStatusCode.OK);

        // First GET after PUT: must be MISS again
        using var afterUpdate = await host.GetResponseAsync("/crud/items/5");
        await afterUpdate.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("MISS", afterUpdate.Headers.GetValues("X-Cache").FirstOrDefault());
    }
}
