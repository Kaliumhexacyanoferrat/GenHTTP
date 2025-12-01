using System.Buffers;
using GenHTTP.Modules.Websockets.Imperative;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Utils;

namespace GenHTTP.Modules.Websockets.Reactive;

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
    internal Func<WebsocketStream, WebsocketFrame, ValueTask>? OnPing { get; set; }
    internal Func<WebsocketStream, ValueTask>? OnPong { get; set; }
    internal Func<WebsocketStream, WebsocketFrame, ValueTask>? OnClose { get; set; }
    internal Func<WebsocketStream, FrameError, ValueTask<bool>>? OnError { get; set; }

    protected override async ValueTask HandleAsync(WebsocketStream target)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(_rxBufferSize);

        try
        {
            if (OnConnected != null) await OnConnected(target);

            while (true)
            {
                var frame = await target.ReadAsync(buffer);

                if (frame.Type == FrameType.Error)
                {
                    if (OnError != null)
                    {
                        if (await OnError(target, frame.FrameError!))
                        {
                            if (OnClose != null) await OnClose(target, frame);
                            break;
                        }
                    }
                    
                    continue;
                }

                if (frame.Type == FrameType.Close)
                {
                    if (OnClose != null) await OnClose(target, frame);
                    break;
                }

                switch (frame.Type)
                {
                    case FrameType.Text:
                        if (OnMessage != null) await OnMessage(target, frame);
                        continue;
                    case FrameType.Ping:
                        if (OnPing != null) await OnPing(target, frame);
                        continue;
                    case FrameType.Pong:
                        if (OnPong != null) await OnPong(target);
                        continue;
                    case FrameType.Continue:
                        if (OnContinue != null) await OnContinue(target, frame);
                        continue;
                    case FrameType.Binary:
                        if (OnBinary != null) await OnBinary(target, frame);
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