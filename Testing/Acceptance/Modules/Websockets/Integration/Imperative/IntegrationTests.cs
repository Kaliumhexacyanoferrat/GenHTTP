using System.Buffers;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Imperative;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestServer()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .CreateImperative()
            .Handler(new MyHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.Execute(host.Port);
    }

    public class MyHandler : IImperativeHandler
    {

        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            var arrayPool = ArrayPool<byte>.Shared;
            var buffer = arrayPool.Rent(8192);

            await connection.PingAsync();

            while (connection.Request.Server.Running)
            {
                var frame = await connection.ReadAsync(buffer);

                if (frame.Type == FrameType.Error)
                {
                    var error = frame.FrameError!;
                    Console.WriteLine($"{error.ErrorType}: {error.Message}");
                    continue;
                }

                if (frame.Type == FrameType.Pong)
                {
                    continue;
                }

                if (frame.Type == FrameType.Close)
                {
                    await connection.CloseAsync();
                    break;
                }

                await connection.WriteAsync(frame.Data, FrameType.Text, fin: true);
            }

            // End
            arrayPool.Return(buffer);
        }

    }

}
