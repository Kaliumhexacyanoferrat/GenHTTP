using GenHTTP.Modules.DependencyInjection;

using Websocket.Client;

using WS = GenHTTP.Modules.Websockets;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;


[TestClass]
public class ImperativeWebsocketTests
{

    [TestMethod]
    public async Task TestImperativeHandler()
    {
        var websocket = WS.Websocket.Imperative()
                          .DependentHandler<Handler>();

        await using var host = await DependentHost.RunAsync(websocket);

        using var client = new WebsocketClient(new Uri($"ws://localhost:{host.Port}"));

        client.Send("Hi");
    }

    private class Handler(AwesomeService dependency) : WS.IImperativeHandler
    {

        public ValueTask HandleAsync(WS.IImperativeConnection connection)
        {
            Assert.AreEqual("42", dependency.DoWork());

            return ValueTask.CompletedTask;
        }

    }

}
