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
            .OnConnected((stream) =>
            {
                return ValueTask.CompletedTask;
            })
            .OnMessage(async (stream, frame) =>
            {
                await stream.WriteAsync(frame.Data);
            })
            .OnClose(async (stream, frame) =>
            {
                await stream.CloseAsync();
            })
            .OnError((stream, error) =>
            {
                //Debug.WriteLine(error.Message);
                //Debug.WriteLine(error.ErrorType);

                return new ValueTask<bool>(false);
            });
        
        Chain.Works(reactiveWebsocket);
        
        await using var host = await TestHost.RunAsync(reactiveWebsocket);
        
        using var client = new ClientWebSocket();
        
        await client.ConnectAsync(new Uri($"ws://localhost:{host.Port}"), CancellationToken.None);
        
        var bytes = Encoding.UTF8.GetBytes("Hello, World!");
        await client.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        
        var response = await client.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
        
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
    }
}