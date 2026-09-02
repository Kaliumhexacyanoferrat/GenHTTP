using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using GenHTTP.Modules.ReverseProxy.Websocket;

using NSubstitute;

namespace GenHTTP.Testing.Acceptance.Modules.ReverseProxy.WebsocketProxy;

[TestClass]
public sealed class RawWebsocketConnectionTests
{

    [TestMethod]
    public void TestInvalidUrlThrows()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new RawWebsocketConnection("not a valid url"));
    }

    [TestMethod]
    public void TestDefaultPortsResolveForKnownAndUnknownSchemes()
    {
        _ = new RawWebsocketConnection("http://example.com/");
        _ = new RawWebsocketConnection("https://example.com/");
        _ = new RawWebsocketConnection("ws://example.com/");
        _ = new RawWebsocketConnection("wss://example.com/");
        _ = new RawWebsocketConnection("ftp://example.com/"); // unrecognized scheme - falls back to port 0
    }

    [TestMethod]
    public async Task TestUpgradeWithoutInitializeThrows()
    {
        var connection = new RawWebsocketConnection("ws://example.com/");

        // Pipe is null before InitializeStream() runs, so the guard fires before the request is
        // ever touched - passing null is safe here.
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => connection.TryUpgrade(null!));
    }

    [TestMethod]
    public async Task TestUpstreamClosesBeforeHandshakeCompletes()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var acceptTask = Task.Run(async () =>
        {
            using var socket = await listener.AcceptSocketAsync();

            // Send a partial, incomplete handshake response, then close without the terminator.
            await socket.SendAsync("HTTP/1.1 101 Switching"u8.ToArray(), SocketFlags.None);
            socket.Shutdown(SocketShutdown.Send);

            // Give the peer time to observe the clean FIN before we tear the socket down.
            await Task.Delay(200);
        });

        await using var connection = new RawWebsocketConnection($"ws://127.0.0.1:{port}/");
        await connection.InitializeStream();

        var server = Substitute.For<IServer>();
        server.Running.Returns(true);

        var request = new Request();
        request.Apply(server);

        var exception = await Assert.ThrowsExactlyAsync<ProviderException>(() => connection.TryUpgrade(request));

        Assert.AreEqual(ResponseStatus.BadGateway, exception.Status);

        await acceptTask;
    }

    [TestMethod]
    public async Task TestUpstreamResetsBeforeHandshakeCompletes()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var acceptTask = Task.Run(async () =>
        {
            using var socket = await listener.AcceptSocketAsync();

            // Abortive close: LingerOption(true, 0) makes Close() send an RST instead of a FIN,
            // reproducing the ECONNRESET a peer triggers when it tears the connection down while
            // our handshake request is still unread in its receive buffer.
            socket.LingerState = new LingerOption(true, 0);
            socket.Close();
        });

        await using var connection = new RawWebsocketConnection($"ws://127.0.0.1:{port}/");
        await connection.InitializeStream();

        var server = Substitute.For<IServer>();
        server.Running.Returns(true);

        var request = new Request();
        request.Apply(server);

        var exception = await Assert.ThrowsExactlyAsync<ProviderException>(() => connection.TryUpgrade(request));

        Assert.AreEqual(ResponseStatus.BadGateway, exception.Status);

        await acceptTask;
    }

}
