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
            try
            {
                Console.WriteLine("1: HandleAsync entered");

                var arrayPool = ArrayPool<byte>.Shared;
                var buffer = arrayPool.Rent(8192);

                Console.WriteLine("2: BeforePing");

                await connection.PingAsync();

                Console.WriteLine("3: AfterPing");

                while (connection.Request.Server.Running)
                {
                    Console.WriteLine("4: BeforeRead");
                    var frame = await connection.ReadAsync(buffer);
                    Console.WriteLine("5: AfterRead");

                    if (frame.Type == FrameType.Error)
                    {
                        var error = frame.FrameError!;
                        Console.WriteLine($"{error.ErrorType}: {error.Message}");
                        continue;
                    }

                    if (frame.Type == FrameType.Pong)
                    {
                        Console.WriteLine("6: ReceivedPong");
                        continue;
                    }

                    if (frame.Type == FrameType.Close)
                    {
                        Console.WriteLine("7: ReceivedClose");
                        await connection.CloseAsync();
                        Console.WriteLine("8: AfterClose");
                        break;
                    }

                    Console.WriteLine("9: BeforeWrite");
                    await connection.WriteAsync(frame.Data, FrameType.Text, fin: true);
                    Console.WriteLine("10: AfterWrite");
                }

                Console.WriteLine("11: End");

                // End
                arrayPool.Return(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }

}
