using System.Buffers;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo.Contents;

public class MulticastWebsocketContent : WebsocketContent
{
    internal Func<Stream, ValueTask>? OnConnected { get; set; }
    internal Func<Stream, ValueTask>? OnMessage { get; set; }
    internal Func<Stream, ValueTask>? OnBinary { get; set; }
    internal Func<Stream, ValueTask>? OnContinue { get; set; }
    internal Func<Stream, ValueTask>? OnPing { get; set; }
    internal Func<Stream, ValueTask>? OnPong { get; set; }
    internal Func<Stream, ValueTask>? OnClose { get; set; }
    
    public override async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(8192);

        try
        {
            if (OnConnected != null) await OnConnected(target);
            
            while (true)
            {
                var frame = await ReadAsync(target, buffer);

                if (frame.Type == FrameType.Close || frame.Data.IsEmpty)
                {
                    if (OnClose != null) await OnClose(target);
                    break;
                }
                
                switch (frame.Type)
                {
                    case FrameType.Text:
                        if (OnMessage != null) await OnMessage(target);
                        continue;
                    case FrameType.Ping:
                        if (OnPing != null) await OnPing(target);
                        continue;
                    case FrameType.Pong:
                        if (OnPong != null) await OnPong(target);
                        continue;
                    case FrameType.Continue:
                        if (OnContinue != null) await OnContinue(target);
                        continue;
                    case FrameType.Binary:
                        if (OnBinary != null) await OnBinary(target);
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