using System.Buffers;
using GenHTTP.Modules.Straculo.Imperative;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Reactive;

public class ReactiveWebsocketContent : WebsocketContent
{
    private readonly int _rxBufferSize;
    
    public ReactiveWebsocketContent(int rxBufferSize)
    {
        _rxBufferSize =  rxBufferSize;
    }
    
    internal Func<WebsocketStream, ValueTask>? OnConnected { get; set; }
    internal Func<WebsocketStream, WebsocketFrame, ValueTask>? OnMessage { get; set; }
    internal Func<WebsocketStream, WebsocketFrame, ValueTask>? OnBinary { get; set; }
    internal Func<WebsocketStream, WebsocketFrame, ValueTask>? OnContinue { get; set; }
    internal Func<WebsocketStream, ValueTask>? OnPing { get; set; }
    internal Func<WebsocketStream, ValueTask>? OnPong { get; set; }
    internal Func<WebsocketStream, ValueTask>? OnClose { get; set; }
    internal Func<WebsocketStream, FrameError, ValueTask<bool>>? OnError { get; set; }
    
    public override async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(_rxBufferSize);

        try
        {
            if (OnConnected != null) await OnConnected(new WebsocketStream(target));

            while (true)
            {
                var frame = await ReadAsync(target, buffer);

                if (frame.Type == FrameType.Error)
                {
                    if (OnError != null)
                    {
                        if (await OnError(new WebsocketStream(target), frame.FrameError!))
                        {
                            if (OnClose != null) await OnClose(new WebsocketStream(target));
                            break;
                        }
                    }
                    
                    continue;
                }

                if (frame.Type == FrameType.Close || frame.Data.IsEmpty)
                {
                    if (OnClose != null) await OnClose(new WebsocketStream(target));
                    break;
                }

                switch (frame.Type)
                {
                    case FrameType.Text:
                        if (OnMessage != null) await OnMessage(new WebsocketStream(target), frame);
                        continue;
                    case FrameType.Ping:
                        if (OnPing != null) await OnPing(new WebsocketStream(target));
                        continue;
                    case FrameType.Pong:
                        if (OnPong != null) await OnPong(new WebsocketStream(target));
                        continue;
                    case FrameType.Continue:
                        if (OnContinue != null) await OnContinue(new WebsocketStream(target), frame);
                        continue;
                    case FrameType.Binary:
                        if (OnBinary != null) await OnBinary(new WebsocketStream(target), frame);
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