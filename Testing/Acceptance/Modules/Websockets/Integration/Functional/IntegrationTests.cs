using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Functional;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestServerFunctional()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .HandleContinuationFramesManually()
                               .OnConnected(c => c.PingAsync())
                               .OnMessage((c, m) => c.WriteAsync(m.Data))
                               .OnContinue((c, m) => c.WriteAsync(m.Data))
                               .OnPing((c, m) => c.PongAsync(m.Data))
                               .OnClose((c, m) => c.CloseAsync())
                               .OnError((c, e) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.Execute(host.Port);
    }
    
    // Automatic segmented handling
    [TestMethod]
    public async Task TestServerFunctionalSegmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(c => c.PingAsync())
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, m) => c.CloseAsync())
            .OnError((c, e) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteSegmented(host.Port);
    }
    
    // Automatic segmented handling
    // Plus TCP fragmentation
    [TestMethod]
    public async Task TestServerFunctionalFragmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(c => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, m) => c.CloseAsync())
            .OnError((c, e) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmented("127.0.0.1", host.Port);
    }
    
    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    [TestMethod]
    public async Task TestServerFunctionalFragmentedSegmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(c => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, m) => c.CloseAsync())
            .OnError((c, e) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }
    
    // Automatic segmented handling
    // Plus TCP fragmentation
    // Plus segmented message
    // No allocations
    [TestMethod]
    public async Task TestServerFunctionalFragmentedSegmentedNoAllocations()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
            .OnConnected(c => ValueTask.CompletedTask)
            .OnMessage((c, m) => c.WriteAsync(m.Data))
            .OnContinue((c, m) => c.WriteAsync(m.Data))
            .OnPing((c, m) => c.PongAsync(m.Data))
            .OnClose((c, m) => c.CloseAsync())
            .OnError((c, e) => ValueTask.FromResult(false));

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmentedWithContinuationFrames("127.0.0.1", host.Port);
    }
}
