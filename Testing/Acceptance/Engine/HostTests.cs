using System.Net;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class HostTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStart(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.Start();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRestart(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.Restart();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

}
