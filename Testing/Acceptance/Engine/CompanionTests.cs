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
    public async Task TestConsole()
    {
        using var runner = new TestHost(Layout.Create().Build());

        runner.Host.Console().Start();

        using var __ = await runner.GetResponseAsync();
    }

    /// <summary>
    /// As a developer, I want to add custom companions to get notified by server actions.
    /// </summary>
    [TestMethod]
    public async Task TestCustom()
    {
        using var runner = new TestHost(Layout.Create().Build());

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
