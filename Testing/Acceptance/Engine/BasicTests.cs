﻿using System.Net;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class BasicTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBuilder(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.RequestMemoryLimit(128)
              .TransferBufferSize(128)
              .RequestReadTimeout(TimeSpan.FromSeconds(2))
              .Backlog(1);

        runner.Start();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacyHttp(TestEngine engine)
    {
        using var runner = TestHost.Run(Layout.Create(), engine: engine);

        using var client = TestHost.GetClient(protocolVersion: new Version(1, 0));

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConnectionClose(TestEngine engine)
    {
        using var runner = TestHost.Run(Layout.Create(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Connection", "close");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
        Assert.IsTrue(response.Headers.Connection.Contains("Close"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyQuery(TestEngine engine)
    {
        using var runner = TestHost.Run(Layout.Create(), engine: engine);

        using var response = await runner.GetResponseAsync("/?");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task TestKeepalive()
    {
        using var runner = TestHost.Run(Layout.Create());

        using var response = await runner.GetResponseAsync();

        Assert.IsTrue(response.Headers.Connection.Contains("Keep-Alive"));
    }

}
