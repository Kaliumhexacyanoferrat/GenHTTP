using GenHTTP.Modules.DependencyInjection;

using Websocket.Client;

using WS = GenHTTP.Modules.Websockets;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class ReactiveWebsocketTests
{

    [TestMethod]
    public async Task TestReactiveHandler()
    {
        var websocket = WS.Websocket.Reactive()
                          .DependentHandler<Handler>();

        await using var host = await DependentHost.RunAsync(websocket);

        using var client = new WebsocketClient(new Uri($"ws://localhost:{host.Port}"));

        await client.StartOrFail();
    }

    private class Handler(AwesomeService dependency) : WS.IReactiveHandler
    {

        public ValueTask OnConnected(WS.IReactiveConnection connection)
        {
            Assert.AreEqual("42", dependency.DoWork());
            return ValueTask.CompletedTask;
        }

    }

}
