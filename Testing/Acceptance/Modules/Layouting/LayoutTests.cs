using System.Net;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Layouting;

[TestClass]
public sealed class LayoutTests
{

    /// <summary>
    /// As a developer I can define the default route to be devlivered.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetIndex(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Index(Content.From(Resource.FromString("Hello World!")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());

        using var notFound = await runner.GetResponseAsync("/notfound");

        await notFound.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// As a developer I can set a default handler to be used for requests.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestDefaultContent(TestEngine engine)
    {
        var layout = Layout.Create().Add(Content.From(Resource.FromString("Hello World!")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        foreach (var path in new[]
                 {
                     "/something", "/"
                 })
        {
            using var response = await runner.GetResponseAsync(path);

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Hello World!", await response.GetContentAsync());
        }
    }

    /// <summary>
    /// As the developer of a web application, I don't want my application
    /// to produce duplicate content for missing trailing slashes.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestRedirect(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Add("section", Layout.Create().Index(Content.From(Resource.FromString("Hello World!"))));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/section/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());

        using var redirected = await runner.GetResponseAsync("/section");

        await redirected.AssertStatusAsync(HttpStatusCode.MovedPermanently);
        AssertX.EndsWith("/section/", redirected.GetHeader("Location")!);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultiSegmentInName(TestEngine engine)
    {
        var layout = Layout.Create().Add("/api/v1/", Content.From(Resource.FromString("Hello World")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/api/v1/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestSameSegmentTwice()
    {
        var content = Content.From(Resource.FromString("Hello World!"));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            Layout.Create().Add("one", content).Add("one", content);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLazyBuilding(TestEngine engine)
    {
        var inner = Layout.Create();

        var outer = Layout.Create()
                          .Add("inner", inner);

        // add a concern _after_ the inner handler has already been added
        // still has to take effect
        inner.Authentication((_, _) => new());

        await using var host = await TestHost.RunAsync(outer, engine: engine);

        using var response = await host.GetResponseAsync("/inner/");

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

}
