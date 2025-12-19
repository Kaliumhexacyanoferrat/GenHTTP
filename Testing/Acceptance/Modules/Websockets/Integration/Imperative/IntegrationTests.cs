using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Imperative;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestServerImperative()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .HandleContinuationFramesManually()
            .Handler(new MyHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.Execute(host.Port);
    }

    // Automatic segmented handling
    [TestMethod]
    public async Task TestServerImperativeSegmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new MyHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteSegmented(host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    [TestMethod]
    public async Task TestServerImperativeFragmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new MyHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmented("127.0.0.1", host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    [TestMethod]
    public async Task TestServerImperativeFragmentedSegmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new MyHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    // No allocations
    [TestMethod]
    public async Task TestServerImperativeFragmentedSegmentedNoAllocations()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket
            .Imperative()
            .Handler(new MyHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }

    public class MyHandler : IImperativeHandler
    {

        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            try
            {
                await connection.PingAsync();

                while (connection.Request.Server.Running)
                {
                    var frame = await connection.ReadFrameAsync();

                    if (frame.IsError(out var error))
                    {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class MyHandlerFragmented : IImperativeHandler
    {

        public async ValueTask HandleAsync(IImperativeConnection connection)
        {
            try
            {
                while (connection.Request.Server.Running)
                {
                    var frame = await connection.ReadFrameAsync();

                    if (frame.IsError(out var error))
                    {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

}
