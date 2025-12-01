using System.Buffers;
using GenHTTP.Modules.Websockets.Imperative;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Utils;
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
            .Add(new MyWebsocketContent());

        Chain.Works(websocket);
        
        await using var host = await TestHost.RunAsync(websocket);
        
        await Client.Execute(host.Port);
    }
    
    public class MyWebsocketContent : WebsocketContent
    {
        protected override async ValueTask HandleAsync(WebsocketStream target)
        {
            var arrayPool = ArrayPool<byte>.Shared;
            var buffer = arrayPool.Rent(8192);

            await target.PingAsync();

            while (true)
            {
                var frame = await target.ReadAsync(buffer);

                if (frame.Type == FrameType.Error)
                {
                    // Deal with error
                    continue;
                }

                if (frame.Type == FrameType.Pong)
                {
                    continue;
                }
            
                if (frame.Type == FrameType.Close)
                {
                    await target.CloseAsync();
                    break;
                }
                await target.WriteAsync(frame.Data, FrameType.Text, fin: true);
            }
            // End
            arrayPool.Return(buffer);
        }
    }
    
}