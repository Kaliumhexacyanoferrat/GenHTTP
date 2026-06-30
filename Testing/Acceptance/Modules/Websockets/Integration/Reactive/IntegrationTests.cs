using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Reactive;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReactive(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
                               .HandleContinuationFramesManually()
                               .Handler(new ReactiveHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.Execute(host.Port);
    }


    [TestMethod]
    [MultiEngineTest]
    public async Task TestSerialization(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
                               .Serialization(new JsonFormat())
                               .Handler(new ReactiveHandlerSerialized());

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteSerialized(host.Port);
    }

    // Automatic segmented handling
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReactiveSegmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
            .Handler(new ReactiveHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteSegmented(host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReactiveFragmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
            .Handler(new ReactiveHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteFragmented("127.0.0.1", host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReactiveFragmentedSegmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
            .Handler(new ReactiveHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    // No allocations
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReactiveFragmentedSegmentedNoAllocations(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Reactive()
            .DoNotAllocateFrameData()
            .Handler(new ReactiveHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }

    public class ReactiveHandler : IReactiveHandler
    {

        public async ValueTask OnConnected(IReactiveConnection connection) => await connection.PingAsync();

        public async ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnContinue(IReactiveConnection connection, IWebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnPing(IReactiveConnection connection, IWebsocketFrame message) => await connection.PongAsync(message.Data);

        public async ValueTask OnClose(IReactiveConnection connection, IWebsocketFrame message) => await connection.CloseAsync();

        public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error)
        {
            Console.WriteLine($"{error.ErrorType}: {error.Message}");
            return ValueTask.FromResult(false);
        }

    }

    public class ReactiveHandlerFragmented : IReactiveHandler
    {

        public ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;

        public async ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnContinue(IReactiveConnection connection, IWebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnPing(IReactiveConnection connection, IWebsocketFrame message) => await connection.PongAsync(message.Data);

        public async ValueTask OnClose(IReactiveConnection connection, IWebsocketFrame message) => await connection.CloseAsync();

        public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error)
        {
            Console.WriteLine($"{error.ErrorType}: {error.Message}");
            return ValueTask.FromResult(false);
        }

    }

    public class ReactiveHandlerSerialized : IReactiveHandler
    {

        public async ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message)
        {
            var thing = await message.ReadPayloadAsync<Client.SerializedThing>();
            await connection.WritePayloadAsync(thing);
        }

    }

}
