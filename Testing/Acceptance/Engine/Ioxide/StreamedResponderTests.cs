using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// How the response head is shaped for an HTTP/2 stream: the headers both protocols treat as
/// malformed are dropped, everything else is forwarded, and the engine names itself as the server.
/// The reserved names are given in mixed case, so the case-insensitive comparison is exercised too.
/// The response version is asserted to be HTTP/2, so a fall back to HTTP/1.1 - which shapes the head
/// differently - fails the test rather than passing it wrongly.
/// </summary>
[TestClass]
public sealed class StreamedResponderTests
{

    [IoxideTestMethod]
    public async Task TestConnectionSpecificHeadersAreDropped()
    {
        // Every reserved name, in mixed case, alongside two that must survive.
        var handler = new HeaderHandler(response => response
            .Header("X-Custom", "kept")
            .Header("X-Another", "also-kept")
            .Header("Connection", "close")
            .Header("Keep-Alive", "timeout=5")
            .Header("Transfer-Encoding", "chunked")
            .Header("Upgrade", "h2c")
            .Header("Proxy-Connection", "keep-alive")
            .Header("SeRvEr", "should-vanish")
            .Content("body"));

        var response = await SendAsync(handler);

        Assert.AreEqual(HttpVersion.Version20, response.Version);
        Assert.AreEqual("body", await response.Content.ReadAsStringAsync());

        // The two ordinary headers made it through.
        Assert.IsTrue(response.Headers.TryGetValues("X-Custom", out var custom));
        Assert.AreEqual("kept", custom!.Single());

        Assert.IsTrue(response.Headers.TryGetValues("X-Another", out var another));
        Assert.AreEqual("also-kept", another!.Single());

        // The engine names itself; the handler's own Server header was dropped rather than kept alongside.
        Assert.IsTrue(response.Headers.TryGetValues("Server", out var server));
        Assert.AreEqual("ioxide-genhttp", server!.Single());

        // None of the reserved names reached the client.
        Assert.IsFalse(response.Headers.Contains("Connection"));
        Assert.IsFalse(response.Headers.Contains("Keep-Alive"));
        Assert.IsFalse(response.Headers.Contains("Transfer-Encoding"));
        Assert.IsFalse(response.Headers.Contains("Upgrade"));
        Assert.IsFalse(response.Headers.Contains("Proxy-Connection"));
    }

    [IoxideTestMethod]
    public async Task TestOrdinaryHeadersAreForwarded()
    {
        var handler = new HeaderHandler(response => response
            .Header("X-One", "1")
            .Header("X-Two", "2")
            .Content("body"));

        var response = await SendAsync(handler);

        Assert.AreEqual(HttpVersion.Version20, response.Version);

        Assert.IsTrue(response.Headers.TryGetValues("X-One", out var one));
        Assert.AreEqual("1", one!.Single());

        Assert.IsTrue(response.Headers.TryGetValues("X-Two", out var two));
        Assert.AreEqual("2", two!.Single());
    }

    #region Infrastructure

    private static async Task<HttpResponseMessage> SendAsync(IHandler handler)
    {
        var port = (ushort)TestHost.NextPort();

        var server = Host.Create()
                         .Handler(handler)
                         .Bind(IPAddress.Loopback, port, HttpProtocols.Http2);

        await server.StartAsync();

        try
        {
            using var socketsHandler = new SocketsHttpHandler();

            using var client = new HttpClient(socketsHandler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };

            var response = await client.GetAsync($"http://localhost:{port}/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Read the body now, while the server is still up, then hand the message back for assertions.
            await response.Content.LoadIntoBufferAsync();

            return response;
        }
        finally
        {
            await server.StopAsync();
        }
    }

    /// <summary>Builds a response from the given header configuration and returns it.</summary>
    private sealed class HeaderHandler(Func<IResponseBuilder, IResponseBuilder> configure) : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
            => new(configure(request.Respond()).Build());
    }

    #endregion

}
