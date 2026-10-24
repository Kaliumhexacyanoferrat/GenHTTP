using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

/// <summary>
/// A connection built with <c>SynchronizeWrites()</c> serializes every write, the framework's own
/// pongs included, so several threads can write while the client keeps pinging.
/// </summary>
[TestClass]
public sealed class SynchronizedWriteTests
{
    private const int Writers = 4;

    private const int Messages = 250;

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSeveralThreadsWriteWhileTheClientPings(ServerEngine engine)
    {
        var faults = new ConcurrentQueue<Exception>();

        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .SynchronizeWrites()
                               .OnConnected(connection =>
                               {
                                   for (var w = 0; w < Writers; w++)
                                   {
                                       var writer = w;

                                       _ = Task.Run(async () =>
                                       {
                                           try
                                           {
                                               for (var m = 0; m < Messages; m++)
                                               {
                                                   await connection.WriteAsync(Encoding.UTF8.GetBytes($"{writer}:{m}"));
                                               }
                                           }
                                           catch (Exception e)
                                           {
                                               faults.Enqueue(e);
                                           }
                                       });
                                   }

                                   return ValueTask.CompletedTask;
                               });

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        using var client = new ClientWebSocket();

        // pings the server keeps answering from its message pump, while the writers write
        client.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(5);
        client.Options.KeepAliveTimeout = TimeSpan.FromSeconds(10);

        await client.ConnectAsync(new Uri($"ws://localhost:{host.Port}"), cts.Token);

        var next = new int[Writers];
        var buffer = new byte[256];

        for (var i = 0; i < Writers * Messages; i++)
        {
            var result = await client.ReceiveAsync(buffer, cts.Token);

            Assert.AreEqual(WebSocketMessageType.Text, result.MessageType);
            Assert.IsTrue(result.EndOfMessage);

            var parts = Encoding.UTF8.GetString(buffer, 0, result.Count).Split(':');

            var writer = int.Parse(parts[0]);

            // each writer's messages arrive whole and in the order it sent them
            Assert.AreEqual(next[writer]++, int.Parse(parts[1]));
        }

        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", cts.Token);

        Assert.IsTrue(faults.IsEmpty, string.Join(Environment.NewLine, faults));
    }

}
