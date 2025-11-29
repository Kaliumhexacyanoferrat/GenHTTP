using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Straculo.Integration.Reactive;

[TestClass]
public sealed class IntegrationTests
{
    [TestMethod]
    public async Task TestServer()
    {
        var reactiveWebsocket = GenHTTP.Modules.Straculo.Websocket
            .CreateReactive(rxBufferSize: 1024)
            // Ping when connection is established, tests outgoing ping
            .OnConnected(async stream => await stream.PingAsync())
            // Test incoming pings, very rare, browsers do not ping
            .OnPing(async (stream, frame) => await stream.PongAsync(frame.Data))
            .OnPong(stream => ValueTask.CompletedTask)
            .OnMessage(async (stream, frame) => await stream.WriteAsync(frame.Data))
            // Also very rarely used feature
            .OnContinue(async (stream, frame) => await stream.WriteAsync(frame.Data))
            .OnClose(async (stream, frame) => await stream.CloseAsync())
            .OnError((stream, error) => new ValueTask<bool>(false));
        
        Chain.Works(reactiveWebsocket);
        
        await using var host = await TestHost.RunAsync(reactiveWebsocket);

        await Client.Execute(host.Port);
    }
}