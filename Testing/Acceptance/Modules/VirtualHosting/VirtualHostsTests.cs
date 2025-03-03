﻿using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.VirtualHosting;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.VirtualHosting;

[TestClass]
public sealed class VirtualHostsTests
{

    /// <summary>
    /// As a hoster, I would like to provide several domains using the
    /// same server instance.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestDomains(TestEngine engine)
    {
        var hosts = VirtualHosts.Create()
                                .Add("domain1.com", Content.From(Resource.FromString("domain1.com")))
                                .Add("domain2.com", Content.From(Resource.FromString("domain2.com")))
                                .Default(Layout.Create().Index(Content.From(Resource.FromString("default"))));

        await using var runner = await TestHost.RunAsync(hosts, engine: engine);

        await RunTest(runner, "domain1.com");
        await RunTest(runner, "domain2.com");

        await RunTest(runner, "localhost", "default");
    }

    /// <summary>
    /// As a developer, I expect the server to return no content if
    /// no given route matches.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoDefault(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(VirtualHosts.Create(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public void TestConcernChaining()
    {
        Chain.Works(VirtualHosts.Create());
    }

    private static async Task RunTest(TestHost runner, string host, string? expected = null)
    {
        var request = runner.GetRequest();
        request.Headers.Add("Host", host);

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual(expected ?? host, await response.GetContentAsync());
    }
}
