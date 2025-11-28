# GenHTTP Module Straculo

This module provides HTTP/1.1 RFC-6455 functionality 


## Imperative

The prefered way to use this module, gives the user absolute control and ownership of the websocket flow.

### Via Webservices/Functional Handler

```cs
// Functional Handler approach
var functionalHandler = Inline
    .Create()
    .Get((IRequest request) => Websocket.CreateWebsocketResponse(request, new MyWebsocketContent()));

var layoutBuilder = Layout.Create()
    .Add("functional", functionalHandler)
    .AddService<MyWebsocketService>("websocket");

await Host.Create()
        .Port(8080)
        .Handler(layoutBuilder)
        .Defaults()
        .RunAsync(); // or StartAsync() for non-blocking

public class MyWebsocketService
{
    [ResourceMethod]
    public IResponse MyWebsocketHandler(IRequest request)
    {
        // Resolve DI dependencies if needed which can be passed to MyWebsocketContent
        
        return Websocket.CreateWebsocketResponse(request, new MyWebsocketContent());
    }
}

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
```

### Using Websocket API

Does not require creating a webservice, functional handler or even a layout.

```cs
var websocket = Websocket
    .Create()
    .Add(new MyWebsocketContent())
    .Build();

await Host.Create()
        .Port(8080)
        .Handler(websocket)
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
```

## Reactive

```cs
var reactiveWebsocket = Websocket
    .CreateReactive()
    .OnConnected((stream) => 
    {
        //OnConnected logic
        
        stream.WriteAsync("Hello from the server."); 

        return ValueTask.CompletedTask;
    })
    .OnMessage(async (stream) =>
    {
        stream.WriteAsync("Hello, World!");   
    })
    .OnClose((stream) =>
    {
        //OnClose logic
        
        return ValueTask.CompletedTask;
    })
    .Build();

await Host.Create()
        .Port(8080)
        .Handler(reactiveWebsocket)
        .Defaults()
        .RunAsync(); // or StartAsync() for non-blocking
```

There are also
 - OnPing
 - OnPong
 - OnBinary
 - OnContinue

 ### Broadcast example

 ```cs
var websocketStreams = new List<WebsocketStream>();

var reactiveWebsocket = Websocket
    .CreateReactive()
    .OnConnected((stream) => 
    {
        websocketStreams.Add(stream);
        return ValueTask.CompletedTask;
    })
    .OnMessage(async (stream) =>
    {
        // Broadcast
        foreach (var websocketStream in websocketStreams)
        {
            await websocketStream.WriteAsync("Hello, World!");
        }
    })
    .OnClose((stream) =>
    {
        websocketStreams.Remove(stream);
        return ValueTask.CompletedTask;
    })
    .Build();

await Host.Create()
          .Port(8080)
          .Handler(reactiveWebsocket)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking
```