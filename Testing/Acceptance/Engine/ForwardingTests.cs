using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ForwardingTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardedHeaderIsRead(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("Forwarded", "for=85.192.1.5; host=google.com; proto=https");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("85.192.1.5"), forwardings[0].For);
        Assert.AreEqual("google.com", forwardings[0].Host);
        Assert.AreEqual(ClientProtocol.Https, forwardings[0].Protocol);
        Assert.IsNull(forwardings[0].By);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardedHeaderSupportsMultipleHops(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("Forwarded", "for=85.192.1.1;by=10.0.0.1, for=85.192.1.2;host=google.com;proto=http");
        });

        Assert.AreEqual(2, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("85.192.1.1"), forwardings[0].For);
        Assert.AreEqual(IPAddress.Parse("10.0.0.1"), forwardings[0].By);

        Assert.AreEqual(IPAddress.Parse("85.192.1.2"), forwardings[1].For);
        Assert.AreEqual("google.com", forwardings[1].Host);
        Assert.AreEqual(ClientProtocol.Http, forwardings[1].Protocol);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardedHeaderSupportsQuotedIPv6(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("Forwarded", "for=\"[2001:db8:cafe::17]:4711\"");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("2001:db8:cafe::17"), forwardings[0].For);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacyHeadersAreRead(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("X-Forwarded-For", "85.192.1.5");
            request.Headers.Add("X-Forwarded-Host", "google.com");
            request.Headers.Add("X-Forwarded-Proto", "http");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("85.192.1.5"), forwardings[0].For);
        Assert.AreEqual("google.com", forwardings[0].Host);
        Assert.AreEqual(ClientProtocol.Http, forwardings[0].Protocol);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacyHeadersSupportMultipleHops(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("X-Forwarded-For", "85.192.1.5, 10.0.0.1");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("85.192.1.5"), forwardings[0].For);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardedHeaderTakesPrecedenceOverLegacy(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("Forwarded", "for=85.192.1.1; host=google.com; proto=https");

            request.Headers.Add("X-Forwarded-For", "85.192.1.2");
            request.Headers.Add("X-Forwarded-Host", "google2.com");
            request.Headers.Add("X-Forwarded-Proto", "http");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.AreEqual(IPAddress.Parse("85.192.1.1"), forwardings[0].For);
        Assert.AreEqual("google.com", forwardings[0].Host);
        Assert.AreEqual(ClientProtocol.Https, forwardings[0].Protocol);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalidValuesAreIgnored(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, request =>
        {
            request.Headers.Add("Forwarded", "for=not-an-ip; host=google.com; proto=not-a-protocol");
        });

        Assert.AreEqual(1, forwardings.Count);

        Assert.IsNull(forwardings[0].For);
        Assert.AreEqual("google.com", forwardings[0].Host);
        Assert.IsNull(forwardings[0].Protocol);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoHeadersResultInEmptyList(TestEngine engine)
    {
        var forwardings = await GetForwardingsAsync(engine, _ => { });

        Assert.AreEqual(0, forwardings.Count);
    }

    private static async Task<List<Forwarding>> GetForwardingsAsync(TestEngine engine, Action<HttpRequestMessage> configure)
    {
        List<Forwarding>? forwardings = null;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            forwardings = r.Header.Headers.GetForwardings();

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        configure(request);

        using var _ = await runner.GetResponseAsync(request);

        return forwardings ?? [];
    }

}
