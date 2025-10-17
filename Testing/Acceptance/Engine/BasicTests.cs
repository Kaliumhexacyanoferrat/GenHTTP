using System.Net;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class BasicTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBuilder(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.RequestMemoryLimit(128)
              .TransferBufferSize(128)
              .RequestReadTimeout(TimeSpan.FromSeconds(2))
              .Backlog(1);

        await runner.StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

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
    public async Task TestNoKeepAliveHeaderOn11()
    {
        await using var runner = await TestHost.RunAsync(Layout.Create());

        using var response = await runner.GetResponseAsync();

        Assert.DoesNotContain("Keep-Alive", response.Headers.Connection);
    }

}
