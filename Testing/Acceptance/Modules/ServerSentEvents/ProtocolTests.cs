using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.ServerSentEvents;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class ProtocolTests
{
    private const string NL = "\n";

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public Task TestComment(ServerEngine engine) => TestAsync(engine, async c => await c.CommentAsync("invisible"), ": invisible");

    [TestMethod]
    [MultiEngineTest]
    public Task TestRetry(ServerEngine engine) => TestAsync(engine, async c => await c.RetryAsync(10000), "retry: 10000");

    [TestMethod]
    [MultiEngineTest]
    public Task TestRetryTwice(ServerEngine engine) => TestAsync(engine, async c =>
    {
        await c.RetryAsync(10000);
        await c.RetryAsync(1);
    }, "retry: 10000");

    [TestMethod]
    [MultiEngineTest]
    public Task TestType(ServerEngine engine) => TestAsync(engine, async c => await c.DataAsync("data", eventType: "TYPE"), $"event: TYPE{NL}data: data");

    [TestMethod]
    [MultiEngineTest]
    public Task TestId(ServerEngine engine) => TestAsync(engine, async c => await c.DataAsync("data", eventId: "4711"), $"id: 4711{NL}data: data");

    [TestMethod]
    [MultiEngineTest]
    public Task TestNull(ServerEngine engine) => TestAsync(engine, async c => await c.DataAsync((int?)null), $"data: ");

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResume(ServerEngine engine)
    {
        var source = EventSource.Create()
                                .Inspector((r, id) =>
                                {
                                    Assert.AreEqual("4711", id);
                                    return new(true);
                                })
                                .Generator(c =>
                                {
                                    Assert.AreEqual("4711", c.LastEventId);
                                    return new();
                                });

        await using var host = await TestHost.RunAsync(source, engine: engine);

        var request = host.GetRequest();

        request.Headers.Add("Last-Event-ID", "4711");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoContent(ServerEngine engine)
    {
        var source = EventSource.Create()
                                .Inspector((r, id) => new (false))
                                .Generator(c =>
                                {
                                     Assert.Fail("Must not happen");
                                     return new();
                                });

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineTest]
    public Task TestException(ServerEngine engine) => TestAsync(engine, c => throw new InvalidOperationException("Nope"), $"retry: 30000");

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetOnly(ServerEngine engine)
    {
        var source = EventSource.Create()
                                .Generator(_ => new());

        await using var host = await TestHost.RunAsync(source, engine: engine);

        var request = host.GetRequest(method: HttpMethod.Head);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);

        Assert.AreEqual("GET", response.GetContentHeader("Allow"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestAccess(ServerEngine engine)
    {
        var source = EventSource.Create()
                                .Generator(c =>
                                {
                                    Assert.IsNotNull(c.Request);
                                    return new();
                                });

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    private static async Task TestAsync(ServerEngine engine, Func<IEventConnection, ValueTask> generator, string expected)
    {
        var source = EventSource.Create()
                                .Generator(generator);

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual($"{expected}{NL}{NL}", await response.GetContentAsync());
    }

    #endregion

}
