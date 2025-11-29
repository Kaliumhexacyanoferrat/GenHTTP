using System.Buffers;
using System.Diagnostics;
using GenHTTP.Modules.Straculo.Imperative;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Straculo.Utils;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Straculo.Integration.Imperative;

[TestClass]
public sealed class IntegrationTests
{
    [TestMethod]
    public async Task TestServer()
    {
        var websocket = GenHTTP.Modules.Straculo.Websocket
            .Create()
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

            while (true)
            {
                var frame = await target.ReadAsync(buffer);

                if (frame.Type == FrameType.Error)
                {
                    // Deal with error
                    Debug.WriteLine(frame.FrameError!.Message);
                    Debug.WriteLine(frame.FrameError!.ErrorType);
                    continue;
                }

                if (frame.Type == FrameType.Close || frame.Data.IsEmpty)
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