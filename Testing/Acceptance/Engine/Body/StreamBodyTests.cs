using System.Net;
using System.Net.Sockets;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class StreamBodyTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSmallContentLength(ServerEngine engine)
    {
        var payload = "Hello, body!"u8.ToArray();

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new ByteArrayContent(payload)
        };

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSmallChunked(ServerEngine engine)
    {
        var payload = "Hello, chunked body!"u8.ToArray();

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(payload))
        };

        request.Headers.TransferEncodingChunked = true;

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeContentLength(ServerEngine engine)
    {
        var payload = new byte[256 * 1024];
        new Random(42).NextBytes(payload);

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new ByteArrayContent(payload)
        };

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeChunked(ServerEngine engine)
    {
        var payload = new byte[256 * 1024];
        new Random(42).NextBytes(payload);

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(payload))
        };

        request.Headers.TransferEncodingChunked = true;

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestKeepAliveContentLength(ServerEngine engine)
    {
        var first = "first request body"u8.ToArray();
        var second = "second request body"u8.ToArray();

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var req1 = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl()) { Content = new ByteArrayContent(first) };

        using var resp1 = await client.SendAsync(req1);

        await resp1.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(first, await resp1.Content.ReadAsByteArrayAsync());

        var req2 = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl()) { Content = new ByteArrayContent(second) };

        using var resp2 = await client.SendAsync(req2);

        await resp2.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(second, await resp2.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestKeepAliveChunked(ServerEngine engine)
    {
        var first = "first chunked request body"u8.ToArray();
        var second = "second chunked request body"u8.ToArray();

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var req1 = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(first))
        };

        req1.Headers.TransferEncodingChunked = true;

        using var resp1 = await client.SendAsync(req1);

        await resp1.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(first, await resp1.Content.ReadAsByteArrayAsync());

        var req2 = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(second))
        };

        req2.Headers.TransferEncodingChunked = true;

        using var resp2 = await client.SendAsync(req2);

        await resp2.AssertStatusAsync(HttpStatusCode.OK);

        CollectionAssert.AreEqual(second, await resp2.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTruncatedContentLength(ServerEngine engine)
    {
        var handler = new TruncationProbe();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = new TcpClient("127.0.0.1", runner.Port)
        {
            ReceiveTimeout = Constants.ClientTimeout
        };

        var stream = client.GetStream();

        await stream.WriteAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 100\r\n\r\nshort"u8.ToArray());

        client.Client.Shutdown(SocketShutdown.Send);

        using var reader = new StreamReader(stream, leaveOpen: true);

        var response = await reader.ReadToEndAsync();

        Assert.IsFalse(response.Contains("HTTP/1.1 200", StringComparison.Ordinal), response);

        // Kestrel usually drops the request on FIN before the handler is invoked
        if (engine == ServerEngine.Kestrel)
        {
            if (handler.Outcome.Task.IsCompleted)
            {
                Assert.IsNotNull(await handler.Outcome.Task, "The handler read a truncated body as a complete one");
            }

            return;
        }

        var failure = await handler.Outcome.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var providerException = failure as ProviderException;

        Assert.IsNotNull(providerException, failure?.ToString() ?? "The handler read a truncated body as a complete one");
        Assert.AreEqual(ResponseStatus.BadRequest, providerException.Status);

        // ioxide closes a connection on FIN, so only the internal engine can still answer
        if (engine == ServerEngine.Internal)
        {
            Assert.IsTrue(response.StartsWith("HTTP/1.1 400", StringComparison.Ordinal), response);
        }
    }

    #endregion

    #region Supporting types

    private sealed class BodyEchoHandler : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var body = request.GetBody();

            if (body is null)
            {
                return request.Respond().Status(ResponseStatus.BadRequest).Build();
            }

            var stream = body.AsStream();

            using var buffer = new MemoryStream();

            await stream.CopyToAsync(buffer);

            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(buffer.ToArray(), ContentType.ApplicationOctetStream)
                          .Build();
        }
    }

    /// <summary>
    /// Reads the body as a stream and records whether this failed.
    /// </summary>
    private sealed class TruncationProbe : IHandler
    {
        public TaskCompletionSource<Exception?> Outcome { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var body = request.GetBody() ?? throw new InvalidOperationException("Request has no body");

            try
            {
                await body.AsStream().CopyToAsync(Stream.Null);
            }
            catch (Exception e)
            {
                Outcome.TrySetResult(e);
                throw;
            }

            Outcome.TrySetResult(null);

            return request.Respond().Status(ResponseStatus.Ok).Build();
        }
    }

    #endregion

}
