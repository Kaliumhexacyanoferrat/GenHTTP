using System.Net.WebSockets;
using System.Text;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.ReverseProxy;
using GenHTTP.Modules.Websockets;

var content = Content.From(Resource.FromString("Hello World!"));

// Server
//
var proxyHost = Host.Create()
          .Port(8080)
          .Handler(Proxy
              .Create()
              .Upstream("ws://localhost:5000/"))
          .Defaults()
          .StartAsync(); // or StartAsync() for non-blocking
//
// Upstream
var websocket = Websocket
    .Create()
    .OnOpen(connection =>
    {
        Console.WriteLine("[Upstream] - Connected");
        return Task.CompletedTask;
    })
    .OnMessage(async (connection, message) =>
    {
        Console.WriteLine($"[Upstream] - Echoing: {message}");
        await connection.SendAsync(message);
    })
    .OnClose(connection =>
    {
        Console.WriteLine("[Upstream] - Closed");
        return Task.CompletedTask;
    })
    .Build();
        
var upstreamHost = Host
    .Create()
    .Port(5000)
    .Handler(websocket)
    .StartAsync();
//
// Downstream
await Task.Run(async () =>
{
    var client = new ClientWebSocket();

    await client.ConnectAsync(new Uri($"ws://localhost:{8080}"), CancellationToken.None);

    while (true)
    {
        var message = "Hello from the other side?"u8.ToArray();

        Console.WriteLine($"[Downstream] - Sending: {message}]");
        
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