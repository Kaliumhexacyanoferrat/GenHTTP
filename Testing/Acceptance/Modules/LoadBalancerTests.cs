using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.LoadBalancing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class LoadBalancerTests
{

    [TestMethod]
    public async Task TestProxy()
    {
        using var upstream = TestHost.Run(Content.From(Resource.FromString("Proxy!")));

        var loadbalancer = LoadBalancer.Create()
                                       .Proxy($"http://localhost:{upstream.Port}");

        using var runner = TestHost.Run(loadbalancer);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Proxy!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestRedirect()
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Redirect("http://node");

        using var runner = TestHost.Run(loadbalancer);

        using var response = await runner.GetResponseAsync("/page");

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("http://node/page", response.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestCustomHandler()
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("My Content!")));

        using var runner = TestHost.Run(loadbalancer);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("My Content!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestPriorities()
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("Prio A")), r => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio B")), r => Priority.Low);

        using var runner = TestHost.Run(loadbalancer);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("Prio A", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestMultiplePriorities()
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("Prio A1")), r => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio A2")), r => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio A3")), r => Priority.High);

        using var runner = TestHost.Run(loadbalancer);

        using var response = await runner.GetResponseAsync();

        AssertX.StartsWith("Prio A", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestNoNodes()
    {
        using var runner = TestHost.Run(LoadBalancer.Create());

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }
}
