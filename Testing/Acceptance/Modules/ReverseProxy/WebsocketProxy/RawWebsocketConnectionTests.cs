using System.Net;
using System.Net.Sockets;
using System.Text;
using GenHTTP.Modules.ReverseProxy.WebsocketTunnel;

namespace GenHTTP.Testing.Acceptance.Modules.ReverseProxy.WebsocketProxy;

[TestClass]
public class RawWebsocketConnectionTests
{
    [TestMethod]
    public void Constructor_ParsesWsUrl_CorrectHostPortRouteAndSecure()
    {
        // Arrange
        var url = "ws://example.com:1234/chat/socket";

        // Act
        var conn = new RawWebsocketConnection(url);

        // Assert (use reflection because fields are private)
        var hostField  = typeof(RawWebsocketConnection).GetField("_host",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var portField  = typeof(RawWebsocketConnection).GetField("_port",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var secureField= typeof(RawWebsocketConnection).GetField("_secure",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var routeField = typeof(RawWebsocketConnection).GetField("_route", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.IsNotNull(hostField);
        Assert.IsNotNull(portField);
        Assert.IsNotNull(secureField);
        Assert.IsNotNull(routeField);

        Assert.AreEqual("example.com", hostField!.GetValue(conn));
        Assert.AreEqual(1234,          portField!.GetValue(conn));
        Assert.IsFalse((bool?)secureField!.GetValue(conn)); // ws → not secure
        Assert.AreEqual("/chat/socket",routeField!.GetValue(conn));
    }

    [TestMethod]
    public void Constructor_DefaultPorts_AreCorrect()
    {
        // Act
        var ws  = new RawWebsocketConnection("ws://example.com/");
        var wss = new RawWebsocketConnection("wss://example.com/");

        var wsPortField   = typeof(RawWebsocketConnection).GetField("_port",   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var wssPortField  = typeof(RawWebsocketConnection).GetField("_port",   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var wsSecureField = typeof(RawWebsocketConnection).GetField("_secure", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var wssSecureField= typeof(RawWebsocketConnection).GetField("_secure", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.AreEqual(80,  wsPortField!.GetValue(ws));
        Assert.AreEqual(443, wssPortField!.GetValue(wss));
        Assert.IsFalse((bool?)wsSecureField!.GetValue(ws));
        Assert.IsTrue((bool?)wssSecureField!.GetValue(wss));
    }

    [TestMethod]
    public async Task InitializeStream_ConnectsAndCreatesStreamAndPipe()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverAcceptTask = AcceptAndDoNothingAsync(listener); // just accept and keep connection open

        var url = $"ws://127.0.0.1:{port}/";
        var conn = new RawWebsocketConnection(url);

        await conn.InitializeStream();

        Assert.IsNotNull(conn.Stream, "Stream should be initialized.");
        Assert.IsNotNull(conn.Pipe,   "Pipe should be initialized.");

        await conn.DisposeAsync();
        listener.Stop();
        await serverAcceptTask;
    }

    [TestMethod]
    public async Task TryUpgrade_Valid101Response_ReturnsTrue_AndSendsCorrectHost()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        string? receivedRequest = null;

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync(cts.Token);
            await using var stream = client.GetStream();

            // Read request headers
            receivedRequest = await ReadHeadersAsync(stream, cts.Token);

            // Send 101 Switching Protocols
            var response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "\r\n";
            var responseBytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cts.Token);
            await stream.FlushAsync(cts.Token);
        }, cts.Token);

        var url = $"ws://127.0.0.1:{port}/ws";
        var conn = new RawWebsocketConnection(url);

        await conn.InitializeStream();

        var clientHeaders = new Dictionary<string, string>
        {
            { "Upgrade", "websocket" },
            { "Connection", "Upgrade" },
            { "Sec-WebSocket-Key", "testkey==" },
            { "Sec-WebSocket-Version", "13" },
            // This Host must be ignored in favor of URL host:port
            { "Host", "evil-host:6666" }
        };

        var ok = await conn.TryUpgrade(clientHeaders, cts.Token);

        Assert.IsTrue(ok, "TryUpgrade should return true for 101 response.");
        Assert.IsNotNull(receivedRequest, "Server should have received an upgrade request.");

        // Sanity: request starts with GET /ws HTTP/1.1
        StringAssert.StartsWith(receivedRequest!, "GET /ws HTTP/1.1");

        // Host must match URL (127.0.0.1:port) and not the clientHeaders "evil-host:9999"
        StringAssert.Contains(receivedRequest!, $"Host: 127.0.0.1:{port}");
        Assert.DoesNotContain("Host: evil-host:9999", receivedRequest);

        await conn.DisposeAsync();
        listener.Stop();
        await serverTask;
    }

    [TestMethod]
    public async Task TryUpgrade_Non101Response_ReturnsFalse()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync(cts.Token);
            await using var stream = client.GetStream();

            // Read request headers (but ignore content)
            _ = await ReadHeadersAsync(stream, cts.Token);

            // Send 500 response instead of 101
            var response =
                "HTTP/1.1 500 Internal Server Error\r\n" +
                "Content-Length: 0\r\n" +
                "\r\n";
            var responseBytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cts.Token);
            await stream.FlushAsync(cts.Token);
        }, cts.Token);

        var url = $"ws://127.0.0.1:{port}/";
        var conn = new RawWebsocketConnection(url);

        await conn.InitializeStream();

        var clientHeaders = new Dictionary<string, string>
        {
            { "Upgrade", "websocket" },
            { "Connection", "Upgrade" },
            { "Sec-WebSocket-Key", "testkey==" },
            { "Sec-WebSocket-Version", "13" }
        };

        var ok = await conn.TryUpgrade(clientHeaders, cts.Token);

        Assert.IsFalse(ok, "TryUpgrade should return false for non-101 response.");

        await conn.DisposeAsync();
        listener.Stop();
        await serverTask;
    }

    private static async Task AcceptAndDoNothingAsync(TcpListener listener)
    {
        using var client = await listener.AcceptTcpClientAsync();
        // Just hold the connection open for a bit
        await Task.Delay(1000);
    }

    private static async Task<string> ReadHeadersAsync(Stream stream, CancellationToken token)
    {
        var buffer = new byte[1024];
        using var ms = new MemoryStream();

        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
            if (read == 0)
            {
                break; // connection closed
            }

            ms.Write(buffer, 0, read);

            var text = Encoding.ASCII.GetString(ms.ToArray());
            if (text.Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                return text;
            }
        }

        return Encoding.ASCII.GetString(ms.ToArray());
    }
}