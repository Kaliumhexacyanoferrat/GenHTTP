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

            if (frame.Type == FrameType.Error)
            {
                // Deal with error
                Debug.WriteLine(frame.FrameError!.Message);
                Debug.WriteLine(frame.FrameError!.ErrorType);
                continue;
            }

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

            if (frame.Type == FrameType.Error)
            {
                // Deal with error
                Debug.WriteLine(frame.FrameError!.Message);
                Debug.WriteLine(frame.FrameError!.ErrorType);
                continue;
            }

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
    .CreateReactive(rxBufferSize: 1024)
    .OnConnected((stream) => 
    {
        //OnConnected logic
        
        stream.WriteAsync("Hello from the server."); 

        return ValueTask.CompletedTask;
    })
    .OnMessage(async (stream, frame) =>
    {
        stream.WriteAsync(frame.Data);   
    })
    .OnClose((stream) =>
    {
        //OnClose logic

        return ValueTask.CompletedTask;
    })
    .OnError((stream, error) =>
    {
        Debug.WriteLine(error.Message);
        Debug.WriteLine(error.ErrorType);
        
        // return true to force connection to close
        return new ValueTask<bool>(false);
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
    .CreateReactive(rxBufferSize: 1024)
    .OnConnected((stream) => 
    {
        websocketStreams.Add(stream);
        return ValueTask.CompletedTask;
    })
    .OnMessage(async (stream, frame) =>
    {
        // Broadcast
        foreach (var websocketStream in websocketStreams)
        {
            await websocketStream.WriteAsync(frame.Data);
        }
    })
    .OnClose((stream) =>
    {
        websocketStreams.Remove(stream);
        return ValueTask.CompletedTask;
    })
    .OnError((stream, error) =>
    {
        Debug.WriteLine(error.Message);
        Debug.WriteLine(error.ErrorType);
        
        // return true to force connection to close
        return new ValueTask<bool>(false);
    })
    .Build();

await Host.Create()
          .Port(8080)
          .Handler(reactiveWebsocket)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking
```

## Fragmented frames

Fragmented frames are supported.

For received frames check the FrameType for FrameType.Continue

For sending frames, set the fin flag

```cs
await WriteAsync(target, frame.Data, FrameType.Text, fin: false);
```
