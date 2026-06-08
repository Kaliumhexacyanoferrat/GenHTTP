using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class DrainBodyTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIgnoredContentLength(TestEngine engine)
    {
        var handler = new CountingHandler();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new ByteArrayContent("ignored body"u8.ToArray())
            };

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIgnoredChunked(TestEngine engine)
    {
        var handler = new CountingHandler();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new StreamContent(new MemoryStream("ignored chunked body"u8.ToArray()))
            };

            request.Headers.TransferEncodingChunked = true;

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIgnoredLargeContentLength(TestEngine engine)
    {
        var payload = new byte[256 * 1024];
        new Random(42).NextBytes(payload);

        var handler = new CountingHandler();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new ByteArrayContent(payload)
            };

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIgnoredLargeChunked(TestEngine engine)
    {
        var payload = new byte[256 * 1024];
        new Random(42).NextBytes(payload);

        var handler = new CountingHandler();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new StreamContent(new MemoryStream(payload))
            };

            request.Headers.TransferEncodingChunked = true;

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPartialRead(TestEngine engine)
    {
        var handler = new PartialReadHandler(bytesToRead: 4);

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new ByteArrayContent("Hello, body!"u8.ToArray())
            };

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    #endregion

    #region Supporting types

    private sealed class CountingHandler : IHandler
    {
        private int _requestCount;

        public int RequestCount => _requestCount;

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Interlocked.Increment(ref _requestCount);

            return ValueTask.FromResult<IResponse?>(
                request.Respond().Status(ResponseStatus.Ok).Build());
        }
    }

    private sealed class PartialReadHandler(int bytesToRead) : IHandler
    {
        private int _requestCount;

        public int RequestCount => _requestCount;

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Interlocked.Increment(ref _requestCount);

            var body = request.GetBody();

            if (body is not null)
            {
                var buffer = new byte[bytesToRead];
                await body.AsStream().ReadExactlyAsync(buffer);
            }

            return request.Respond().Status(ResponseStatus.Ok).Build();
        }
    }

    #endregion

}
