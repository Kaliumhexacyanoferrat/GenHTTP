using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Engine.Shared.Types;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Straculo;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Webservices;

var content = Content.From(Resource.FromString("Hello World!"));

var layoutBuilder = Layout.Create()
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
        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key");
        
        var myWebsocket = new MyWebsocket(request);

        var response = new Response();

        response.Status = new FlexibleResponseStatus(101, "Switching Protocols");
        
        response.Headers.Add("Upgrade", "websocket");
        response.Headers.Add("Connection", "Upgrade");
        if (key is not null)
        {
            response.Headers.Add("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key));
        }

        response.Content = myWebsocket;

        return response;
    }
}

public class MyWebsocket : Websocket
{
    public MyWebsocket(IRequest request) : base(request)
    {
    }

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
            
            await WriteAsync(target, frame.Data, 0x01);
        }
        
        // End
        
        arrayPool.Return(buffer);
    }
}
