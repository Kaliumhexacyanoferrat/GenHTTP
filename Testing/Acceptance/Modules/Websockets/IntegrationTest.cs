using WS = GenHTTP.Modules.Websockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Websocket.Client;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public sealed class IntegrationTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServer(TestEngine engine)
    {
        var waitEvent = new ManualResetEvent(false);

        var length = 0;

        var server = WS.Websocket.Create()
                       .OnMessage((socket, msg) =>
                       {
                           length += msg.Length;
                           socket.Send(msg);
                           socket.Close();
                       });

        await using var host = await TestHost.RunAsync(server, engine: engine);

        using var client = new WebsocketClient(new Uri("ws://localhost:" + host.Port));

        client.MessageReceived.Subscribe(msg =>
        {
            length += msg.Text?.Length ?? 0;
            waitEvent.Set();
        });

        await client.Start();

        _ = Task.Run(() => client.Send("1234567890"));

        Assert.IsTrue(waitEvent.WaitOne(TimeSpan.FromSeconds(5)));

        Assert.AreEqual(20, length);
    }

}
