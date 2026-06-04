using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class MemoryBodyTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSmallContentLength(TestEngine engine)
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
    public async Task TestSmallChunked(TestEngine engine)
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
    public async Task TestLargeContentLength(TestEngine engine)
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
    public async Task TestLargeChunked(TestEngine engine)
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
    public async Task TestKeepAliveContentLength(TestEngine engine)
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
    public async Task TestKeepAliveChunked(TestEngine engine)
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

    #endregion

    #region Supporting types

    private sealed class BodyEchoHandler : IHandler
    {
        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var body = request.GetBody();

            if (body is null)
            {
                return request.Respond().Status(ResponseStatus.BadRequest).Build();
            }

            var bytes = (await body.AsMemoryAsync()).ToArray();

            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(bytes, ContentType.ApplicationOctetStream)
                          .Build();
        }
    }

    #endregion

}
