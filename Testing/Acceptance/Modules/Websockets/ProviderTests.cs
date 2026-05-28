using System.Buffers;
using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public class ProviderTests
{

    [TestMethod]
    public async Task TestHandshake()
    {
        var handler = new WebsocketHandler((r) => new FlushingContent());

        await using var runner = await TestHost.RunAsync(handler);

        var request = runner.GetRequest();
        
        request.Headers.Add("Upgrade", "websocket");
        request.Headers.Add("Connection", "upgrade");
        request.Headers.Add("Sec-WebSocket-Key", "x3JJHMbDL1EzLkh9GBhXDw==");
        request.Headers.Add("Sec-WebSocket-Version", "13");
        
        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.SwitchingProtocols);

        Assert.AreEqual("websocket", response.GetHeader("Upgrade"));
        Assert.AreEqual("HSmrc0sMlYUkAGmm5OPpG2HaGWk=", response.GetHeader("Sec-WebSocket-Accept"));
    }
    
    [TestMethod]
    public async Task TestBadHandshake()
    {
        var content = new ResourceContent(Resource.FromString("Hello World").Build());
        
        var handler = new WebsocketHandler((r) => content);

        await using var runner = await TestHost.RunAsync(handler);
        
        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task TestBadMethod()
    {
        var content = new ResourceContent(Resource.FromString("Hello World").Build());
        
        var handler = new WebsocketHandler((r) => content);

        await using var runner = await TestHost.RunAsync(handler);
        
        var request = runner.GetRequest();

        request.Method = HttpMethod.Head;
        
        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);
    }
    
    [TestMethod]
    public async Task TestBadVersion()
    {
        var content = new ResourceContent(Resource.FromString("Hello World").Build());
        
        var handler = new WebsocketHandler((r) => content);

        await using var runner = await TestHost.RunAsync(handler);

        var request = runner.GetRequest();
        
        request.Headers.Add("Upgrade", "websocket");
        request.Headers.Add("Connection", "upgrade");
        request.Headers.Add("Sec-WebSocket-Key", "x3JJHMbDL1EzLkh9GBhXDw==");
        request.Headers.Add("Sec-WebSocket-Version", " 15");
        
        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.UpgradeRequired);
    }

    private class FlushingContent : IResponseContent
    {

        public ulong? Length => 11;

        public ContentType? Type => null;

        public ReadOnlyMemory<byte>? Encoding => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => new(42);

        public async ValueTask WriteAsync(IResponseSink sink)
        {
            await sink.Stream.WriteAsync("Hello World"u8.ToArray());

            await sink.Stream.FlushAsync();
        }
        
    }
    
}
