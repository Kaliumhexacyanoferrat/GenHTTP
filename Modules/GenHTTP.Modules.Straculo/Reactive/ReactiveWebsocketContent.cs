using System.Buffers;
using GenHTTP.Modules.Straculo.Imperative;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo.Reactive;

public class ReactiveWebsocketContent : WebsocketContent
{
    internal Func<ReactiveWebsocketStream, ValueTask>? OnConnected { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnMessage { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnBinary { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnContinue { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnPing { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnPong { get; set; }
    internal Func<ReactiveWebsocketStream, ValueTask>? OnClose { get; set; }
    
    public override async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(8192);

        try
        {
            if (OnConnected != null) await OnConnected(new ReactiveWebsocketStream(target));
            
            while (true)
            {
                var frame = await ReadAsync(target, buffer);

                if (frame.Type == FrameType.Close || frame.Data.IsEmpty)
                {
                    if (OnClose != null) await OnClose(new ReactiveWebsocketStream(target));
                    break;
                }
                
                switch (frame.Type)
                {
                    case FrameType.Text:
                        if (OnMessage != null) await OnMessage(new ReactiveWebsocketStream(target));
                        continue;
                    case FrameType.Ping:
                        if (OnPing != null) await OnPing(new ReactiveWebsocketStream(target));
                        continue;
                    case FrameType.Pong:
                        if (OnPong != null) await OnPong(new ReactiveWebsocketStream(target));
                        continue;
                    case FrameType.Continue:
                        if (OnContinue != null) await OnContinue(new ReactiveWebsocketStream(target));
                        continue;
                    case FrameType.Binary:
                        if (OnBinary != null) await OnBinary(new ReactiveWebsocketStream(target));
                        continue;
                    default:
                        break;
                }
            }
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }
}