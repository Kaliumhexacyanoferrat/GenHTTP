using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketContent(IReactiveHandler handler, IRequest request, int rxBufferSize) : IResponseContent
{

    public ulong? Length => null;

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;

        var buffer = arrayPool.Rent(rxBufferSize);

        var connection = new WebsocketConnection(request, target);

        try
        {
            await handler.OnConnected(connection);

            while (true)
            {
                var frame = await connection.ReadAsync(buffer);

                if (frame.Type == FrameType.Error)
                {
                    if (await handler.OnError(connection, frame.FrameError!))
                    {
                        await handler.OnClose(connection, frame);
                        break;
                    }

                    continue;
                }

                if (frame.Type == FrameType.Close)
                {
                    await handler.OnClose(connection, frame);
                    break;
                }

                switch (frame.Type)
                {
                    case FrameType.Text:
                        await handler.OnMessage(connection, frame);
                        continue;
                    case FrameType.Ping:
                        await handler.OnPing(connection, frame);
                        continue;
                    case FrameType.Pong:
                        await handler.OnPong(connection, frame);
                        continue;
                    case FrameType.Continue:
                        await handler.OnContinue(connection, frame);
                        continue;
                    case FrameType.Binary:
                        await handler.OnBinary(connection, frame);
                        continue;
                }
            }
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

}
