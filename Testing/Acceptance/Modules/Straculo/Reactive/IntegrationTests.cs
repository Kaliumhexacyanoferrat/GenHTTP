using System.Net.WebSockets;
using System.Text;
using GenHTTP.Testing.Acceptance.Utilities;
using Websocket.Client;

namespace GenHTTP.Testing.Acceptance.Modules.Straculo.Reactive;

[TestClass]
public sealed class IntegrationTests
{
    [TestMethod]
    public async Task TestServer()
    {
        var reactiveWebsocket = GenHTTP.Modules.Straculo.Websocket
            .CreateReactive(rxBufferSize: 1024)
            .OnConnected(async stream => await stream.PingAsync() )
            .OnPong(async stream => await stream.WriteAsync("Nice Pong!"u8.ToArray()))
            .OnMessage(async (stream, frame) => await stream.WriteAsync(frame.Data))
            .OnPing(async (stream, frame) => await stream.PongAsync(frame.Data))
            .OnClose(async (stream, frame) => await stream.CloseAsync())
            .OnError((stream, error) => new ValueTask<bool>(false));
        
        Chain.Works(reactiveWebsocket);
        
        await using var host = await TestHost.RunAsync(reactiveWebsocket);
        
        using var client = new ClientWebSocket();
        
        await client.ConnectAsync(new Uri($"ws://localhost:{host.Port}"), CancellationToken.None);
        
        // Sending a Text message
        var bytes = Encoding.UTF8.GetBytes("Hello, World!");
        await client.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        
        var response = await client.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
        
        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(bytes));
        
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
    }
}