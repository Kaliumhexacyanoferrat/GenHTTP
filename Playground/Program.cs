using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Straculo;
using GenHTTP.Modules.Straculo.Imperative;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Straculo.Provider;
using GenHTTP.Modules.Straculo.Reactive;
using GenHTTP.Modules.Webservices;

var content = Content.From(Resource.FromString("Hello World!"));

var layoutBuilder = Layout.Create()
    .AddService<MyWebsocketService>("websocket");

var websocket = Websocket
    .Create()
    .Add(new MyWebsocketContent())
    .Build();

var streams = new List<ReactiveWebsocketStream>();

var reactiveWebsocket = Websocket
    .CreateReactive()
    .OnConnected((stream) => 
    {
        streams.Add(stream);
        return ValueTask.CompletedTask;
    })
    .OnMessage(async (stream) =>
    {
        // Broadcast
        foreach (var strm in streams)
        {
            await strm.WriteAsync("Hello World!");
        }
    })
    .Build();

await Host.Create()
          .Port(8080)
          .Handler(reactiveWebsocket)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking

public class MyWebsocketContent : WebsocketContent
{
    public override async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(8192);

        while (true)
        {
            var frame = await ReadAsync(target, buffer);
            if (frame.Type == FrameType.Close || frame.Data.IsEmpty)
            {
                break;
            }
            await WriteAsync(target, frame.Data, FrameType.Text);
        }
        // End
        arrayPool.Return(buffer);
    }
}


public class MyWebsocketService
{
    [ResourceMethod]
    public IResponse MyWebsocketHandler(IRequest request)
    {
        // Resolve DI dependencies if needed which can be passed to MyWebsocketContent
        
        return Websocket.CreateWebsocketResponse(request, new MyWebsocketContent());
    }
}

