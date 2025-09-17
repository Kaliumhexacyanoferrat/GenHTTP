using System.Net;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Layouting;

[TestClass]
public class MultiSegmentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRegular(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(["api", "v1"], Content.From(Resource.FromString("Hello API!")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/api/v1/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello API!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestExisting(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(["api", "v1", "first"], Content.From(Resource.FromString("First")))
                        .Add(["api", "v1", "second"], Content.From(Resource.FromString("Second")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/api/v1/second");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Second", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoSegments(TestEngine engine)
    {
        var app = Layout.Create().Add([], Content.From(Resource.FromString("Content")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Content", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestSegmentOccupied()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            Layout.Create()
                  .Add("api", Redirect.To("https://example.com"))
                  .Add([ "api", "v1" ], Content.From(Resource.FromString("API")));
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmpty(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add([], Content.From(Resource.FromString("Hello Empty!")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello Empty!", await response.GetContentAsync());
    }

}
