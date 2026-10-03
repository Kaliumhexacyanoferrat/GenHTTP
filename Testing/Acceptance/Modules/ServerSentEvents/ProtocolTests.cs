using System.Net;
using System.Net.Sockets;
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

    /// <summary>
    /// As a client closes the connection, the generator learns that it is gone from the next
    /// event it sends, so a source that runs until nobody listens comes to an end.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGeneratorNoticesClosedConnection(ServerEngine engine)
    {
        var left = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var source = EventSource.Create()
                                .Generator(async c =>
                                {
                                    while (await c.CommentAsync("ping"))
                                    {
                                        await Task.Delay(50);
                                    }

                                    Assert.IsFalse(c.Connected);
                                    left.TrySetResult();
                                });

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using (var client = new TcpClient())
        {
            await client.ConnectAsync("127.0.0.1", host.Port);

            var stream = client.GetStream();

            await stream.WriteAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n"u8.ToArray());

            // wait for the first event, so the generator is running when the client leaves
            var buffer = new byte[1024];
            var received = "";

            while (!received.Contains(": ping"))
            {
                var read = await stream.ReadAsync(buffer);
                Assert.AreNotEqual(0, read);
                received += System.Text.Encoding.ASCII.GetString(buffer, 0, read);
            }
        }

        var finished = await Task.WhenAny(left.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.AreSame(left.Task, finished, "The generator kept sending after the client closed the connection");
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
