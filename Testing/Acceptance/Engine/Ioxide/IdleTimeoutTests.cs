using System.Net;
using System.Net.Sockets;
using System.Text;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Functional;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The TCP idle timeout runs only while the server waits on its client, so an idle keep-alive
/// connection is closed, while a handler that takes longer than the timeout keeps its connection.
/// </summary>
[TestClass]
public sealed class IdleTimeoutTests
{

    private static readonly EngineOptions Options = new()
    {
        Reactor = new ReactorOptions { ReactorCount = 2 },
        Tcp = new TcpTransportOptions { IdleTimeoutMs = 500 },
    };

    [IoxideTestMethod]
    public async Task TestAnIdleConnectionIsClosed()
    {
        var app = Inline.Create().Get(() => "ok");

        var port = (ushort)TestHost.NextPort();

        var server = Host.Create(options: Options).Handler(app).Bind(IPAddress.Loopback, port);

        await server.StartAsync();

        try
        {
            using var client = new TcpClient();

            await client.ConnectAsync(IPAddress.Loopback, port);

            var stream = client.GetStream();

            await stream.WriteAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n"u8.ToArray());

            var response = await ReadResponseAsync(stream);

            StringAssert.StartsWith(response, "HTTP/1.1 200");

            // nothing more is sent, so the connection waits on the client from here on
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            Assert.AreEqual(0, await stream.ReadAsync(new byte[1], timeout.Token), "The idle connection was not closed");
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestASlowHandlerIsNotCut()
    {
        var app = Inline.Create().Get(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return "slow";
        });

        var port = (ushort)TestHost.NextPort();

        var server = Host.Create(options: Options).Handler(app).Bind(IPAddress.Loopback, port);

        await server.StartAsync();

        try
        {
            using var client = new HttpClient();

            Assert.AreEqual("slow", await client.GetStringAsync($"http://localhost:{port}/"));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    // Reads one response with a known length off the connection, leaving it open.
    private static async Task<string> ReadResponseAsync(NetworkStream stream)
    {
        var buffer = new byte[4096];
        var received = new StringBuilder();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (true)
        {
            var read = await stream.ReadAsync(buffer, timeout.Token);

            Assert.AreNotEqual(0, read, "The connection closed before the response arrived");

            received.Append(Encoding.ASCII.GetString(buffer, 0, read));

            var text = received.ToString();

            var end = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);

            if (end >= 0 && text.EndsWith("ok", StringComparison.Ordinal))
            {
                return text;
            }
        }
    }

}
