using System.Net;

using GenHTTP.Modules.ServerSentEvents;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class ProtocolTests
{
    private const string NL = "\n";

    #region Tests

    [TestMethod]
    public Task TestComment() => TestAsync(async (c) => await c.CommentAsync("invisible"), ": invisible");

    [TestMethod]
    public Task TestRetry() => TestAsync(async (c) => await c.RetryAsync(10000), "retry: 10000");

    [TestMethod]
    public Task TestRetryTwice() => TestAsync(async (c) =>
    {
        await c.RetryAsync(10000);
        await c.RetryAsync(1);
    }, "retry: 10000");

    [TestMethod]
    public Task TestType() => TestAsync(async (c) => await c.DataAsync("data", eventType: "TYPE"), $"event: TYPE{NL}data: data");

    [TestMethod]
    public Task TestId() => TestAsync(async (c) => await c.DataAsync("data", eventId: "4711"), $"id: 4711{NL}data: data");

    [TestMethod]
    public Task TestNull() => TestAsync(async (c) => await c.DataAsync((int?)null), $"data: ");

    [TestMethod]
    public async Task TestResume()
    {
        var source = EventSource.Create()
                                .Inspector((r, id) =>
                                {
                                    Assert.AreEqual("4711", id);
                                    return new(true);
                                })
                                .Generator((c) =>
                                {
                                    Assert.AreEqual("4711", c.LastEventId);
                                    return new();
                                });

        using var host = TestHost.Run(source);

        var request = host.GetRequest();

        request.Headers.Add("Last-Event-ID", "4711");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestNoContent()
    {
        var source = EventSource.Create()
                                .Inspector((r, id) => new (false))
                                .Generator((c) =>
                                {
                                     Assert.Fail("Must not happen");
                                     return new();
                                });

        using var host = TestHost.Run(source);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public Task TestException() => TestAsync((c) => throw new InvalidOperationException("Nope"), $"retry: 30000");

    [TestMethod]
    public async Task TestGetOnly()
    {
        var source = EventSource.Create()
                                .Generator((_) => new());

        using var host = TestHost.Run(source);

        var request = host.GetRequest(method: HttpMethod.Head);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);
    }

    [TestMethod]
    public async Task TestRequestAccess()
    {
        var source = EventSource.Create()
                                .Generator((c) =>
                                {
                                    Assert.IsNotNull(c.Request);
                                    return new();
                                });

        using var host = TestHost.Run(source);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    private static async Task TestAsync(Func<IEventConnection, ValueTask> generator, string expected)
    {
        var source = EventSource.Create()
                                .Generator(generator);

        using var host = TestHost.Run(source);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual($"{expected}{NL}{NL}", await response.GetContentAsync());
    }

    #endregion

}
