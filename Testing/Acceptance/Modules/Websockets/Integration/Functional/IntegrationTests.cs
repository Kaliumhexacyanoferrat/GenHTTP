using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Functional;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestServerReactive()
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

        await Client.Execute(host.Port);
    }

}
