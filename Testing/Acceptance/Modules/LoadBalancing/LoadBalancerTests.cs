using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.LoadBalancing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.LoadBalancing;

[TestClass]
public sealed class LoadBalancerTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestProxy(TestEngine engine)
    {
        await using var upstream = await TestHost.RunAsync(Content.From(Resource.FromString("Proxy!")), engine: engine);

        var loadbalancer = LoadBalancer.Create()
                                       .Proxy($"http://localhost:{upstream.Port}");

        await using var runner = await TestHost.RunAsync(loadbalancer);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Proxy!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRedirect(TestEngine engine)
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Redirect("http://node");

        await using var runner = await TestHost.RunAsync(loadbalancer, engine: engine);

        using var response = await runner.GetResponseAsync("/page");

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("http://node/page", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomHandler(TestEngine engine)
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("My Content!")));

        await using var runner = await TestHost.RunAsync(loadbalancer, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("My Content!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPriorities(TestEngine engine)
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("Prio A")), _ => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio B")), _ => Priority.Low);

        await using var runner = await TestHost.RunAsync(loadbalancer, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("Prio A", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultiplePriorities(TestEngine engine)
    {
        var loadbalancer = LoadBalancer.Create()
                                       .Add(Content.From(Resource.FromString("Prio A1")), _ => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio A2")), _ => Priority.High)
                                       .Add(Content.From(Resource.FromString("Prio A3")), _ => Priority.High);

        await using var runner = await TestHost.RunAsync(loadbalancer, engine: engine);

        using var response = await runner.GetResponseAsync();

        AssertX.StartsWith("Prio A", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoNodes(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(LoadBalancer.Create(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }
}
