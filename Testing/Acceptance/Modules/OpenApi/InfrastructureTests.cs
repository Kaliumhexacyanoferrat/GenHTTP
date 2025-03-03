﻿using System.Net;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class InfrastructureTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentIsPassed(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/some-path", () => "Hello World")
                        .AddOpenApi();

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync("/some-path");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCaching(TestEngine engine) => Assert.AreEqual(1, await RunCachingTest(engine, true));

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoCaching(TestEngine engine) => Assert.AreEqual(2, await RunCachingTest(engine, false));

    private async Task<int> RunCachingTest(TestEngine engine, bool cacheEnabled)
    {
        var counter = 0;

        var description = ApiDescription.Create().Caching(cacheEnabled).PostProcessor((_, _) => counter++);

        var api = Inline.Create()
                        .Get(() => "Hello World")
                        .Add(description);

        await using var host = await TestHost.RunAsync(api, engine: engine);

        AssertX.Contains("\"openapi\"", await (await host.GetResponseAsync("/openapi.json")).GetContentAsync());
        AssertX.Contains("openapi:", await (await host.GetResponseAsync("/openapi.yaml")).GetContentAsync());

        return counter;
    }

}
