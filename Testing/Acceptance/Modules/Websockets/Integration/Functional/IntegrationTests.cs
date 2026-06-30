using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Websockets;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Functional;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerFunctional(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .HandleContinuationFramesManually()
                               .OnConnected(c => c.PingAsync())
                               .OnMessage((c, m) => c.WriteAsync(m.Data))
                               .OnContinue((c, m) => c.WriteAsync(m.Data))
                               .OnPing((c, m) => c.PongAsync(m.Data))
                               .OnClose((c, _) => c.CloseAsync())
                               .OnError((_, _) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.Execute(host.Port);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestText(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .Formatters(Formatting.Default().Build())
                               .OnMessage(async (c, m) =>
                               {
                                   var data = await m.ReadPayloadAsync<string>();
                                   await c.WritePayloadAsync(data);
                               });

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteSerialized(host.Port);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSerialization(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .Serialization(new JsonFormat())
                               .OnMessage(async (c, m) =>
                               {
                                   var thing = await m.ReadPayloadAsync<Client.SerializedThing>();
                                   await c.WritePayloadAsync(thing);
                               });

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteSerialized(host.Port);
    }

    // Automatic segmented handling
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerFunctionalSegmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(c => c.PingAsync())
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, _) => c.CloseAsync())
            .OnError((_, _) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteSegmented(host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerFunctionalFragmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(_ => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, _) => c.CloseAsync())
            .OnError((_, _) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteFragmented("127.0.0.1", host.Port);
    }

    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerFunctionalFragmentedSegmented(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(_ => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, _) => c.CloseAsync())
            .OnError((_, _) => ValueTask.FromResult(false));

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
    public async Task TestServerFunctionalFragmentedSegmentedNoAllocations(TestEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(_ => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, _) => c.CloseAsync())
            .OnError((_, _) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }

}
