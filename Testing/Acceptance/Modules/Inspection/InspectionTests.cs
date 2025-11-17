using System.Net;

using GenHTTP.Modules.Inspection;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Inspection;

[TestClass]
public sealed class InspectionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInspection(TestEngine engine)
    {
        var app = Content.From(Resource.FromString("Hello World")).AddInspector();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.OK);

        var content = await inspected.GetContentAsync();

        AssertX.Contains("/one/two", content);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoInspection(TestEngine engine)
    {
        var app = Content.From(Resource.FromString("Hello World")).AddInspector();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var notInspected = await host.GetResponseAsync("/one/two");

        await notInspected.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await notInspected.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFoundInspected(TestEngine engine)
    {
        var app = Layout.Create().AddInspector();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("/one/two", await inspected.GetContentAsync());
    }

}
