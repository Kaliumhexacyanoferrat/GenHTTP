using System.Net;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class HostTests
{

    [TestMethod]
    public void TestPortZeroThrows()
    {
        var host = Host.Create();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => host.Port(0));
    }

    [TestMethod]
    public async Task TestRunAsyncReturnsErrorCodeWithoutHandler()
    {
        var host = Host.Create();

        var exitCode = await host.RunAsync();

        Assert.AreEqual(-1, exitCode);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStart(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        await runner.Host.StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRestart(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        await runner.Host.RestartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

}
