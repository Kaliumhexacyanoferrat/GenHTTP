using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Testing.Acceptance.Modules.Websockets.Integration;

using WS = GenHTTP.Modules.Websockets;
using IWS = GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Imperative;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;


[TestClass]
public class ImperativeWebsocketTests
{

    [TestMethod]
    public async Task TestReactiveHandler()
    {
        var websocket = WS.Websocket.Imperative()
                          .DependentHandler<Handler>();

        await using var host = await DependentHost.RunAsync(websocket);

        await Client.Execute(host.Port);
    }

    private class Handler(AwesomeService dependency) : WS.IImperativeHandler
    {

        public ValueTask HandleAsync(WS.IImperativeConnection connection)
        {
            Assert.AreEqual("42", dependency.DoWork());

            return new IWS.IntegrationTests.MyHandler().HandleAsync(connection);
        }

    }

}
