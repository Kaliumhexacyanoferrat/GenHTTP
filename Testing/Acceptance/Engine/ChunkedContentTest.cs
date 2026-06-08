using System.Net;
using System.Net.Http.Json;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ChunkedContentTest
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestChunkedUpload(TestEngine engine)
    {
        var inline = Inline.Create()
                           .Put((Model model) => model);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var client = TestHost.GetClient();

        using var response = await client.PutAsJsonAsync(runner.GetUrl(), new Model("Hello World"));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var result = await response.GetContentAsync();

        AssertX.Contains("Hello World", result);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestChunkedBodyIsRead(TestEngine engine)
    {
        const string payload = "Hello, chunked world!";

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload)))
        };

        request.Headers.TransferEncodingChunked = true;

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var received = await response.Content.ReadAsByteArrayAsync();

        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(payload), received);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeChunkedBodyIsRead(TestEngine engine)
    {
        var random = new Random(42);
        var payload = new byte[256 * 1024]; // 256 KB across many chunks
        random.NextBytes(payload);

        await using var runner = await TestHost.RunAsync(new BodyEchoHandler().Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
        {
            Content = new StreamContent(new MemoryStream(payload))
        };

        request.Headers.TransferEncodingChunked = true;

        using var response = await client.SendAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var received = await response.Content.ReadAsByteArrayAsync();

        CollectionAssert.AreEqual(payload, received);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestChunkedBodyIsDrainedWhenNotRead(TestEngine engine)
    {
        // The engine must drain an unread chunked body so the connection can be reused.
        var handler = new DrainVerifyHandler();

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var client = TestHost.GetClient();

        for (int i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, runner.GetUrl())
            {
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ignored body")))
            };

            request.Headers.TransferEncodingChunked = true;

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        Assert.AreEqual(3, handler.RequestCount);
    }

    #endregion

    #region Supporting types

    public record Model(string Value);

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

            var bytes = (await body.AsMemoryAsync()).ToArray();

            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(bytes, ContentType.ApplicationOctetStream)
                          .Build();
        }
    }

    private sealed class DrainVerifyHandler : IHandler
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

    #endregion

}
