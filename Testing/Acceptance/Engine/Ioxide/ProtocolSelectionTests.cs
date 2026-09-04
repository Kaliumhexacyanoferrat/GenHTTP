using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// Which protocol a cleartext port speaks, which is decided by the HTTP/2 connection preface rather
/// than by ALPN.
/// </summary>
[TestClass]
public sealed class ProtocolSelectionTests
{

    [IoxideTestMethod]
    public async Task TestOneSocketServesBothWhenThePortAsksForBoth()
    {
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, HttpProtocols.Http1AndHttp2);

        await server.StartAsync();

        try
        {
            Assert.AreEqual(HttpVersion.Version11, await VersionServedAsync(port, HttpVersion.Version11));
            Assert.AreEqual(HttpVersion.Version20, await VersionServedAsync(port, HttpVersion.Version20));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp2OnlyPortTurnsAwayHttp1()
    {
        // Not HTTP/2 on a port that does not serve HTTP/1.1 either: closed, rather than answered
        // with a protocol the endpoint was configured not to speak.
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, HttpProtocols.Http2);

        await server.StartAsync();

        try
        {
            Assert.AreEqual(HttpVersion.Version20, await VersionServedAsync(port, HttpVersion.Version20));

            await Assert.ThrowsExactlyAsync<HttpRequestException>(async () => await VersionServedAsync(port, HttpVersion.Version11));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp1IsWhatAPortServesByDefault()
    {
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port);

        await server.StartAsync();

        try
        {
            Assert.AreEqual(HttpVersion.Version11, await VersionServedAsync(port, HttpVersion.Version11));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    /// <summary>Asks for exactly one version over cleartext, so h2 goes out as prior knowledge.</summary>
    private static async Task<Version> VersionServedAsync(ushort port, Version version)
    {
        using var handler = new SocketsHttpHandler();

        using var client = new HttpClient(handler)
        {
            DefaultRequestVersion = version,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
            Timeout = TimeSpan.FromSeconds(10),
        };

        using var response = await client.GetAsync($"http://localhost:{port}/ok");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("ok", await response.Content.ReadAsStringAsync());

        return response.Version;
    }

    private static IServerHost Build()
        => Host.Create()
               .Handler(Layout.Create().Add("ok", Content.From(Resource.FromString("ok"))));

}
