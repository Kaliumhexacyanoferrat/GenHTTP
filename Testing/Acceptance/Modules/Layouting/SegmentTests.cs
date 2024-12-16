using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Layouting;

[TestClass]
public class SegmentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSegment(TestEngine engine)
    {
        var app = Layout.Create();

        var section = app.AddSegment("segment");

        section.Index(Content.From(Resource.FromString("Hello World")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/segment/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultiSegment(TestEngine engine)
    {
        var app = Layout.Create();

        var section = app.AddSegments(["one", "two"]);

        section.Index(Content.From(Resource.FromString("Hello World")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/one/two/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

}
