using System.Net.WebSockets;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ReverseProxy;

namespace GenHTTP.Testing.Acceptance.Modules.ReverseProxy;

[TestClass]
public class WebsocketTunnelTests
{
    [TestMethod]
    public async Task TestBasics()
    {
        (TestSetup setup, int port) = await TestSetup.CreateAsync();

        var client = new ClientWebSocket();
        
        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), CancellationToken.None);
        
        var message = "Hello from the other side?"u8.ToArray();
        
        await client.SendAsync(
            message, 
            WebSocketMessageType.Text, 
            true, 
            CancellationToken.None);
        
        var responseBuffer = new byte[message.Length];
        await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);

        Assert.AreEqual("Hello from the other side?", Encoding.UTF8.GetString(message));

        await setup.DisposeAsync();
    }
    
    private class TestSetup : IAsyncDisposable
    {
        private readonly TestHost _target;

        private TestSetup(TestHost source, TestHost target)
        {
            Runner = source;
            _target = target;
        }

        public TestHost Runner { get; }

        public static async Task<(TestSetup, int)> CreateAsync()
        {
            var upstreamServer = new TestHost(Layout.Create().Build(), false);

            // TODO: This logic should be replaced with new websocket module when available
            await upstreamServer.Host.Handler(GenHTTP.Modules.Websockets.Websocket.Create()
                    .OnOpen(connection =>
                    {
                        Console.WriteLine("[Upstream] - Connected");
                        return Task.CompletedTask;
                    })
                    .OnMessage(async (connection, message) =>
                    {
                        Console.WriteLine($"[Upstream] - Echoing: {message}");
                        await connection.SendAsync(message);
                    })
                    .OnClose(connection =>
                    {
                        Console.WriteLine("[Upstream] - Closed");
                        return Task.CompletedTask;
                    }))
                    .StartAsync();

            // proxying server
            var proxy = Proxy.Create()
                //.Upstream("http://localhost:" + upstreamServer.Port);
                .Upstream("wss://ws.postman-echo.com/raw");

            var runner = new TestHost(Layout.Create().Build());

            await runner.Host.Handler(proxy)
                .StartAsync();

            return (new TestSetup(runner, upstreamServer), runner.Port);
        }

        #region IDisposable Support

        private bool _disposedValue;

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    await Runner.DisposeAsync();
                    await _target.DisposeAsync();
                }

                _disposedValue = true;
            }
        }

        public ValueTask DisposeAsync() => DisposeAsync(true);

        #endregion

    }
}