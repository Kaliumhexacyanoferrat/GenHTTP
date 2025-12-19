using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketContent(IReactiveHandler handler, IRequest request, ConnectionSettings settings) : IResponseContent
{

    public ulong? Length => null;

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.FlushAsync();

        await using var connection = new WebsocketConnection(request, target, settings);

        await handler.OnConnected(connection);

        while (request.Server.Running)
        {
            var frame = await connection.ReadFrameAsync(); // Ensures a frame is read

            if (frame.IsError(out var error))
            {
                if (await handler.OnError(connection, error))
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
}
