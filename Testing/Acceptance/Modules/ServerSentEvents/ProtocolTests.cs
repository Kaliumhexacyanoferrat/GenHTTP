using System.Net;

using GenHTTP.Modules.ServerSentEvents;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class ProtocolTests
{
    private const string NL = "\n";

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public Task TestComment(TestEngine engine) => TestAsync(engine, async c => await c.CommentAsync("invisible"), ": invisible");

    [TestMethod]
    [MultiEngineTest]
    public Task TestRetry(TestEngine engine) => TestAsync(engine, async c => await c.RetryAsync(10000), "retry: 10000");

    [TestMethod]
    [MultiEngineTest]
    public Task TestRetryTwice(TestEngine engine) => TestAsync(engine, async c =>
    {
        await c.RetryAsync(10000);
        await c.RetryAsync(1);
    }, "retry: 10000");

    [TestMethod]
    [MultiEngineTest]
    public Task TestType(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync("data", eventType: "TYPE"), $"event: TYPE{NL}data: data");

    [TestMethod]
    [MultiEngineTest]
    public Task TestId(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync("data", eventId: "4711"), $"id: 4711{NL}data: data");

    [TestMethod]
    [MultiEngineTest]
    public Task TestNull(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync((int?)null), $"data: ");

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResume(TestEngine engine)
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
    public async Task TestNoContent(TestEngine engine)
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
    public Task TestException(TestEngine engine) => TestAsync(engine, c => throw new InvalidOperationException("Nope"), $"retry: 30000");

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetOnly(TestEngine engine)
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
    public async Task TestRequestAccess(TestEngine engine)
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

    private static async Task TestAsync(TestEngine engine, Func<IEventConnection, ValueTask> generator, string expected)
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
