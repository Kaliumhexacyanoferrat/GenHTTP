using GenHTTP.Testing.Acceptance.Utilities;

using WS = GenHTTP.Modules.Websockets;

using Websocket.Client;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public sealed class IntegrationTest
{

    [TestMethod]
    public async Task TestServer()
    {
        var waitEvent = new ManualResetEvent(false);

        var length = 0;

        var server = WS.Websocket.Create()
                       .OnMessage(async (socket, msg) =>
                       {
                           length += msg.Length;

                           await socket.SendAsync(msg);

                           socket.Close();
                       });

        Chain.Works(server);

        await using var host = await TestHost.RunAsync(server);

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

    [TestMethod]
    public async Task TestDataTypes()
    {
        var waitEvent = new ManualResetEvent(false);

        var server = WS.Websocket.Create()
                       .OnOpen(async (socket) =>
                       {
                           await socket.SendPingAsync([42]);
                           await socket.SendPongAsync([42]);

                           await socket.SendAsync([42]);

                           Assert.IsTrue(socket.IsAvailable);

                           var request = socket.Request;
                           
                           Assert.IsNotEmpty(request.Headers);
                           
                           Assert.IsEmpty(request.Cookies);
                           
                           Assert.IsEmpty(request.Forwardings);
                           
                           Assert.Contains("localhost", request.Host ?? string.Empty);
                           
                           Assert.Throws<NotSupportedException>(() => request.Upgrade());
                           
                           Assert.Throws<NotSupportedException>(() => request.Respond());
                           
                           socket.Close(42);

                           waitEvent.Set();
                       });

        await using var host = await TestHost.RunAsync(server);

        using var client = new WebsocketClient(new Uri("ws://localhost:" + host.Port));

        await client.Start();

        Assert.IsTrue(waitEvent.WaitOne(TimeSpan.FromSeconds(5)));
    }

}
