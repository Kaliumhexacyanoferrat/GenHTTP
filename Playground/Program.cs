using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
//using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

var content = Content.From(Resource.FromString("Hello World!"));

var reactiveWs =
    Websocket.Reactive()
        .MaxFrameSize(1024)
        .Handler(new ReactiveHandler())
        .Build();

var imperativeWs = 
    Websocket
        .Imperative()
        .Handler(new MyHandler());

await Host.Create()
    .Port(8080)
    .Handler(imperativeWs)
    .Defaults()
    .RunAsync(); // or StartAsync() for non-blocking
    
    
public class ReactiveHandler : IReactiveHandler
{
    public ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;

    public async ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

    public async ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

    public async ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) => await connection.PongAsync(message.Data);

    public async ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) => await connection.CloseAsync();

    public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error)
    {
        Console.WriteLine($"{error.ErrorType}: {error.Message}");
        return ValueTask.FromResult(false);
    }
}

public class MyHandler : IImperativeHandler
{

    public async ValueTask HandleAsync(IImperativeConnection connection)
    {
        try
        {
            // await connection.PingAsync();
            
            while (connection.Request.Server.Running)
            {
                var frame = await connection.ReadFrameAsync();

                if (frame.Type == FrameType.Error)
                {
                    var error = frame.FrameError!;
                    Console.WriteLine($"{error.ErrorType}: {error.Message}");
                    continue;
                }

                if (frame.Type == FrameType.Pong)
                {
                    continue;
                }

                if (frame.Type == FrameType.Close)
                {
                    await connection.CloseAsync();
                    break;
                }

                await connection.WriteAsync(frame.Data, FrameType.Text, fin: true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
