using System.Net;

using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class BasicTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacyHttp(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var client = TestHost.GetClient(protocolVersion: new Version(1, 0));

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConnectionClose(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Connection", "close");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
        Assert.Contains("Close", response.Headers.Connection);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyQuery(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var response = await runner.GetResponseAsync("/?");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoKeepAliveHeaderOn11(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.DoesNotContain("Keep-Alive", response.Headers.Connection);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerHeader(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var response = await runner.GetResponseAsync();

        // Not an exact prefix - engines identify themselves differently (e.g. Kestrel sends
        // "GenHTTP-Kestrel/x.y", Ioxide sends "ioxide-genhttp/x.y"), so just check that the
        // response identifies itself as a GenHTTP server somewhere in the value.
        var server = response.GetHeader("Server");
        Assert.IsTrue(server?.Contains("genhttp", StringComparison.OrdinalIgnoreCase) ?? false, $"Server header '{server}' does not identify a GenHTTP server");
    }

}
