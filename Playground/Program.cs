using System.Net.WebSockets;
using System.Text;

using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Practices;

using GenHTTP.Modules.ReverseProxy;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

// Server
//
var proxyHost = Host.Create()
          .Port(8080)
          .Handler(Proxy
              .Create()
              //.Upstream("wss://ws.postman-echo.com/raw"))
              .Upstream("ws://localhost:5000"))
          .Defaults()
          .StartAsync(); // or StartAsync() for non-blocking

var upstreamServer = Host.Create()
    .Port(5000)
    .Handler(Websocket.Functional()
                .OnMessage((connection, message) =>
                {
                    connection.WriteAsync(
                        message.Data, 
                        FrameType.Text, 
                        true, 
                        CancellationToken.None);
                    
                    return ValueTask.CompletedTask;
                })
        ).StartAsync();

//
// Downstream
await Task.Run(async () =>
{
    var client = new ClientWebSocket();

    await client.ConnectAsync(new Uri($"ws://localhost:{8080}"), CancellationToken.None);

    while (true)
    {
        var message = "Hello from the other side?"u8.ToArray();

        Console.WriteLine($"[Downstream] - Sending: {Encoding.UTF8.GetString(message)}");
        
        await client.SendAsync(
            message, 
            WebSocketMessageType.Text, 
            true, 
            CancellationToken.None);
        
        var responseBuffer = new byte[message.Length];
        await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);

        Console.WriteLine($"[Downstream] - Received: {Encoding.UTF8.GetString(responseBuffer, 0, responseBuffer.Length)}");
        
        await Task.Delay(500);
    }
});