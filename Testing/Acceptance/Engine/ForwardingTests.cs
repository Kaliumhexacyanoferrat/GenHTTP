using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ForwardingTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestModern(TestEngine engine)
    {
        var responder = Inline.Create().Get((IRequest request) => ToString(request.Client));

        await using var host = await TestHost.RunAsync(responder, engine: engine);

        var request = host.GetRequest();

        request.Headers.Add("Forwarded", "for=85.192.1.5; host=google.com; proto=https");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("IPAddress = 85.192.1.5, Protocol = Https, Host = google.com", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacy(TestEngine engine)
    {
        var responder = Inline.Create().Get((IRequest request) => ToString(request.Client));

        await using var host = await TestHost.RunAsync(responder, engine: engine);

        var request = host.GetRequest();

        request.Headers.Add("X-Forwarded-For", "85.192.1.5");
        request.Headers.Add("X-Forwarded-Host", "google.com");
        request.Headers.Add("X-Forwarded-Proto", "http");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("IPAddress = 85.192.1.5, Protocol = Http, Host = google.com", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBoth(TestEngine engine)
    {
        var responder = Inline.Create().Get((IRequest request) => ToString(request.Client));

        await using var host = await TestHost.RunAsync(responder, engine: engine);

        var request = host.GetRequest();

        request.Headers.Add("Forwarded", "for=85.192.1.1; host=google.com; proto=https");

        request.Headers.Add("X-Forwarded-For", "85.192.1.2");
        request.Headers.Add("X-Forwarded-Host", "google2.com");
        request.Headers.Add("X-Forwarded-Proto", "http");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("IPAddress = 85.192.1.1, Protocol = Https, Host = google.com", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalid(TestEngine engine)
    {
        var responder = Inline.Create().Get((IRequest request) => ToString(request.Forwardings.First()));

        await using var host = await TestHost.RunAsync(responder, engine: engine);

        var request = host.GetRequest();

        request.Headers.Add("Forwarded", "for=google.com; host=google.com; proto=google.com");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("For = , Host = google.com, Protocol = ", await response.GetContentAsync());
    }

    private string ToString(IClientConnection connection)
    {
        return $"IPAddress = {connection.IpAddress}, Protocol = {connection.Protocol}, Host = {connection.Host}";
    }

    private string ToString(Forwarding forwarding)
    {
        return $"For = {forwarding.For}, Host = {forwarding.Host}, Protocol = {forwarding.Protocol}";
    }

}
