using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.VirtualHosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class VirtualHostsTests
{

    /// <summary>
    /// As a hoster, I would like to provide several domains using the
    /// same server instance.
    /// </summary>
    [TestMethod]
    public async Task TestDomains()
    {
        var hosts = VirtualHosts.Create()
                                .Add("domain1.com", Content.From(Resource.FromString("domain1.com")))
                                .Add("domain2.com", Content.From(Resource.FromString("domain2.com")))
                                .Default(Layout.Create().Index(Content.From(Resource.FromString("default"))));

        await using var runner = await TestHost.RunAsync(hosts);

        await RunTest(runner, "domain1.com");
        await RunTest(runner, "domain2.com");

        await RunTest(runner, "localhost", "default");
    }

    /// <summary>
    /// As a developer, I expect the server to return no content if
    /// no given route matches.
    /// </summary>
    [TestMethod]
    public async Task TestNoDefault()
    {
        await using var runner = await TestHost.RunAsync(VirtualHosts.Create());

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    private static async Task RunTest(TestHost runner, string host, string? expected = null)
    {
        var request = runner.GetRequest();
        request.Headers.Add("Host", host);

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual(expected ?? host, await response.GetContentAsync());
    }
}
