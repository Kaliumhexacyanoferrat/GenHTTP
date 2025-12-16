using GenHTTP.Engine.Internal;

using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

var websocket = Websocket.Reactive()
    //.HandleContinuationFramesManually()
    .Handler(new ChatHandler());

await Host.Create()
    .Handler(websocket)
    .Defaults()
    .Development()
    .Console()
    .RunAsync();

class ChatHandler : IReactiveHandler
{
    private static readonly List<IReactiveConnection> Clients = [];

    public ValueTask OnConnected(IReactiveConnection connection)
    {
        Clients.Add(connection);
        return ValueTask.CompletedTask;
    }

    public async ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message)
    {
        Console.WriteLine($"Received Ping: {message.DataAsString()}");
        await connection.PongAsync();
    }

    public async ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message)
    {
        var clientNumber = Clients.IndexOf(connection);
        
        foreach (var client in Clients)
        {
            await client.WriteAsync($"[{clientNumber}]: " + message.DataAsString());
        }
    }

    public ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message)
    {
        Console.WriteLine(message.DataAsString());
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message)
    {
        Clients.Remove(connection);
        return ValueTask.CompletedTask;
    }

}