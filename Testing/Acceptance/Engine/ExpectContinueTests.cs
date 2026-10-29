using System.Net;
using System.Net.Sockets;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;

namespace GenHTTP.Testing.Acceptance.Engine;

/// <summary>
/// A client that sends <c>Expect: 100-continue</c> waits for a 100 (Continue) before it sends the
/// body (RFC 9110, section 10.1.1). The engine answers it as the handler starts reading the body, so
/// a handler that answers without reading never invites the upload.
/// </summary>
[TestClass]
public sealed class ExpectContinueTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContinueIsSentAsTheBodyIsRead(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(CountingApp(), engine: engine);

        using var client = await ConnectAsync(host);

        var stream = client.GetStream();

        await stream.WriteAsync("PUT / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\n"u8.ToArray());

        // the body is held back until the server asks for it
        var interim = await ReadHeadAsync(stream);

        StringAssert.StartsWith(interim, "HTTP/1.1 100 Continue");

        await stream.WriteAsync("hello"u8.ToArray());

        var final = await ReadHeadAsync(stream);

        StringAssert.StartsWith(final, "HTTP/1.1 200");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoContinueIfTheBodyIsNotRead(ServerEngine engine)
    {
        var app = Inline.Create().Put(() => "not interested");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var client = await ConnectAsync(host);

        var stream = client.GetStream();

        await stream.WriteAsync("PUT / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\n"u8.ToArray());

        var head = await ReadHeadAsync(stream);

        StringAssert.StartsWith(head, "HTTP/1.1 200");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTheConnectionServesOnIfTheBodyIsSentAnyway(ServerEngine engine)
    {
        // A client that got a final response without a 100 may still send the body,
        // which is then skipped so the connection can serve the next request.
        var app = Inline.Create().Put(() => "not interested");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var client = await ConnectAsync(host);

        var stream = client.GetStream();

        await stream.WriteAsync("PUT / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\n"u8.ToArray());

        var head = await ReadHeadAsync(stream);

        StringAssert.StartsWith(head, "HTTP/1.1 200");

        Assert.AreEqual("not interested", await ReadBodyAsync(stream, ContentLength(head)));

        await stream.WriteAsync("helloPUT / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nhello"u8.ToArray());

        StringAssert.StartsWith(await ReadHeadAsync(stream), "HTTP/1.1 200");
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTheConnectionServesOnAfterTheContinue(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(CountingApp(), engine: engine);

        using var client = await ConnectAsync(host);

        var stream = client.GetStream();

        for (var i = 0; i < 2; i++)
        {
            await stream.WriteAsync("PUT / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\n"u8.ToArray());

            StringAssert.StartsWith(await ReadHeadAsync(stream), "HTTP/1.1 100 Continue");

            await stream.WriteAsync("hello"u8.ToArray());

            StringAssert.StartsWith(await ReadHeadAsync(stream), "HTTP/1.1 200");

            // the counted length, as the body of the response
            Assert.AreEqual("5", await ReadBodyAsync(stream, 1));
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientUploadsWithoutWaiting(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(CountingApp(), engine: engine);

        var request = host.GetRequest(method: HttpMethod.Put);

        request.Headers.ExpectContinue = true;
        request.Content = new ByteArrayContent(new byte[2 * 1024 * 1024]);

        using var handler = new SocketsHttpHandler
        {
            // long enough that waiting it out would fail the test rather than slow it down
            Expect100ContinueTimeout = TimeSpan.FromSeconds(30)
        };

        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual((2 * 1024 * 1024).ToString(), await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnknownExpectationFails(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(CountingApp(), engine: engine);

        var request = host.GetRequest(method: HttpMethod.Put);

        request.Headers.TryAddWithoutValidation("Expect", "something-else");
        request.Content = new StringContent("hello");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.ExpectationFailed);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHttp10IgnoresTheExpectation(ServerEngine engine)
    {
        // An HTTP/1.0 client cannot ask for an interim response, so its body follows right away
        // and no 100 may precede the final response.
        await using var host = await TestHost.RunAsync(CountingApp(), engine: engine);

        using var client = await ConnectAsync(host);

        var stream = client.GetStream();

        await stream.WriteAsync("PUT / HTTP/1.0\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\nhello"u8.ToArray());

        var head = await ReadHeadAsync(stream);

        StringAssert.StartsWith(head, "HTTP/1.1 200");
    }

    #region Helpers

    // Reads the body to the end and answers with its length.
    private static InlineBuilder CountingApp() => Inline.Create().Put(async (Stream body) =>
    {
        var buffer = new byte[64 * 1024];

        long total = 0;
        int read;

        while ((read = await body.ReadAsync(buffer)) > 0)
        {
            total += read;
        }

        return total;
    });

    private static async Task<TcpClient> ConnectAsync(TestHost host)
    {
        var client = new TcpClient();

        await client.ConnectAsync(IPAddress.Loopback, host.Port);

        return client;
    }

    // One response head, byte by byte so nothing of what follows is consumed with it.
    private static async Task<string> ReadHeadAsync(NetworkStream stream)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var head = new StringBuilder();
        var one = new byte[1];

        while (!head.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            var read = await stream.ReadAsync(one, timeout.Token);

            Assert.AreNotEqual(0, read, $"The connection closed after: {head}");

            head.Append((char)one[0]);
        }

        return head.ToString();
    }

    private static async Task<string> ReadBodyAsync(NetworkStream stream, int length)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var body = new byte[length];

        await stream.ReadExactlyAsync(body, timeout.Token);

        return Encoding.ASCII.GetString(body);
    }

    private static int ContentLength(string head)
    {
        foreach (var line in head.Split("\r\n"))
        {
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                return int.Parse(line["Content-Length:".Length..].Trim());
            }
        }

        throw new AssertFailedException($"Response has no Content-Length: {head}");
    }

    #endregion

}
