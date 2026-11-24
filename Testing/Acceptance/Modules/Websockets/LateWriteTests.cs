using System.Net;
using System.Net.WebSockets;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Websockets;

using GenHTTP.Testing.Acceptance.Engine.Ioxide;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

/// <summary>
/// A write made after the handler has ended, as a broadcaster or a timer started from <c>OnConnected</c>
/// makes once the client has gone, is refused instead of reaching a sink the engine is done with.
/// </summary>
[TestClass]
public sealed class LateWriteTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAWriteAfterTheHandlerHasEndedIsRefused(ServerEngine engine)
    {
        var connected = new TaskCompletionSource<IReactiveConnection>(TaskCreationOptions.RunContinuationsAsynchronously);

        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .OnConnected(connection =>
                               {
                                   connected.TrySetResult(connection);
                                   return ValueTask.CompletedTask;
                               });

        await using var host = await TestHost.RunAsync(websocket, engine: engine);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        using var client = new ClientWebSocket();

        await client.ConnectAsync(new Uri($"ws://localhost:{host.Port}"), cts.Token);

        var connection = await connected.Task.WaitAsync(cts.Token);

        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", cts.Token);

        // The handler ends just after the close handshake. Until then a write may still fail on the transport instead.
        Exception? refused = null;

        while (refused is not ConnectionClosedException && !cts.IsCancellationRequested)
        {
            try
            {
                await connection.WriteAsync("late"u8.ToArray());
                await Task.Delay(10);
            }
            catch (Exception e)
            {
                refused = e;
            }
        }

        Assert.IsInstanceOfType<ConnectionClosedException>(refused, $"the late write was not refused: {refused}");
    }

    [IoxideTestMethod]
    public async Task TestALateWriteDoesNotReachTheConnectionThatTookItsPlace()
    {
        // ioxide pools its connections, so on the only reactor a second client takes the first one's place, and a
        // write still going to the first one's sink would land in the second client's stream.
        var first = new TaskCompletionSource<IReactiveConnection>(TaskCreationOptions.RunContinuationsAsynchronously);

        var websocket = GenHTTP.Modules.Websockets.Websocket.Functional()
                               .OnConnected(connection =>
                               {
                                   first.TrySetResult(connection);
                                   return ValueTask.CompletedTask;
                               })
                               .OnMessage((connection, message) => connection.WriteAsync(message.Data));

        var port = (ushort)TestHost.NextPort();

        var options = new EngineOptions { Reactor = new ReactorOptions { ReactorCount = 1 } };

        var server = Host.Create(options: options).Handler(websocket).Bind(IPAddress.Loopback, port);

        await server.StartAsync();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            IReactiveConnection late;

            using (var client = new ClientWebSocket())
            {
                await client.ConnectAsync(new Uri($"ws://localhost:{port}"), cts.Token);

                late = await first.Task.WaitAsync(cts.Token);

                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", cts.Token);
            }

            // Time for the reactor to take the first connection back into its pool.
            await Task.Delay(200, cts.Token);

            using var second = new ClientWebSocket();

            await second.ConnectAsync(new Uri($"ws://localhost:{port}"), cts.Token);

            Exception? refused = null;

            try
            {
                await late.WriteAsync("late"u8.ToArray());
            }
            catch (Exception e)
            {
                refused = e;
            }

            await second.SendAsync("mine"u8.ToArray(), WebSocketMessageType.Text, true, cts.Token);

            var buffer = new byte[64];

            var result = await second.ReceiveAsync(buffer, cts.Token);

            Assert.AreEqual("mine", Encoding.UTF8.GetString(buffer, 0, result.Count), "the second client received another connection's frame");

            Assert.IsInstanceOfType<ConnectionClosedException>(refused, $"the late write was not refused: {refused}");
        }
        finally
        {
            await server.StopAsync();
        }
    }

}
