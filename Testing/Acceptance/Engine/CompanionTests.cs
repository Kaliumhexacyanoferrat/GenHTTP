using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class CompanionTests
{

    /// <summary>
    /// As a developer, I want to configure the server to easily log to the console.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestConsole(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.Console().Start();

        using var __ = await runner.GetResponseAsync();
    }

    /// <summary>
    /// As a developer, I want to add custom companions to get notified by server actions.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustom(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        var companion = new CustomCompanion();

        runner.Host.Companion(companion).Start();

        using var __ = await runner.GetResponseAsync();

        // the companion is called _after_ the response has been sent
        // bad hack, reconsider
        Thread.Sleep(50);

        Assert.IsTrue(companion.Called);
    }

    private class CustomCompanion : IServerCompanion
    {

        public bool Called { get; private set; }

        public void OnRequestHandled(IRequest request, IResponse response)
        {
            Called = true;
        }

        public void OnServerError(ServerErrorScope scope, IPAddress? client, Exception error)
        {
            Called = true;
        }
    }
}
