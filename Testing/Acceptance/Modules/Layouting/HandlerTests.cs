using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Layouting;

[TestClass]
public class HandlerTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHandlerDirect(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Add("section", Content.From(Resource.FromString("Hello World")).Build());

        await using var host = await TestHost.RunAsync(layout, engine: engine);

        using var response = await host.GetResponseAsync("/section/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIndexDirect(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Index(Content.From(Resource.FromString("Hello World")).Build());

        await using var host = await TestHost.RunAsync(layout, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFallbackDirect(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Add(Content.From(Resource.FromString("Hello World")).Build());

        await using var host = await TestHost.RunAsync(layout, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

}
