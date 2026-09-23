using System.Net.WebSockets;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
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
    public async Task TestServerFunctional(ServerEngine engine)
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
    public async Task TestText(ServerEngine engine)
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
    public async Task TestSerialization(ServerEngine engine)
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

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestHeaderAvailableAfterUpgrade(ServerEngine engine)
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .OnMessage(async (c, _) =>
                               {
                                   var header = c.Request.Header;

                                   var name = header.Query.GetEntry("name");
                                   var agent = header.Headers.GetEntry("X-Agent");

                                   await c.WriteAsync(Encoding.UTF8.GetBytes($"{name}|{agent}"));
                               });

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        using var cts = new CancellationTokenSource(4000);

        using var client = new ClientWebSocket();

        client.Options.SetRequestHeader("X-Agent", "tester");

        await client.ConnectAsync(new Uri($"ws://localhost:{host.Port}/?name=alice"), cts.Token);

        await client.SendAsync("ping"u8.ToArray(), WebSocketMessageType.Text, true, cts.Token);

        var buffer = new byte[64];
        var response = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

        Assert.AreEqual("alice|tester", Encoding.UTF8.GetString(buffer, 0, response.Count));
    }

    // Automatic segmented handling
    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerFunctionalSegmented(ServerEngine engine)
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
    public async Task TestServerFunctionalFragmented(ServerEngine engine)
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
    public async Task TestServerFunctionalFragmentedSegmented(ServerEngine engine)
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
    public async Task TestServerFunctionalFragmentedSegmentedNoAllocations(ServerEngine engine)
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
