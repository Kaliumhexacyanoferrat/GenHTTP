using System.Net;

using GenHTTP.Modules.Inspection;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Inspection;

[TestClass]
public sealed class InspectionTests
{

    [TestMethod]
    public async Task TestInspection()
    {
        var app = Content.From(Resource.FromString("Hello World")).AddInspector();

        await using var host = await TestHost.RunAsync(app);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("/one/two", await inspected.GetContentAsync());
    }

    [TestMethod]
    public async Task TestNoInspection()
    {
        var app = Content.From(Resource.FromString("Hello World")).AddInspector();

        await using var host = await TestHost.RunAsync(app);

        using var notInspected = await host.GetResponseAsync("/one/two");

        await notInspected.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await notInspected.GetContentAsync());
    }

    [TestMethod]
    public async Task TestNotFoundInspected()
    {
        var app = Layout.Create().AddInspector();

        await using var host = await TestHost.RunAsync(app);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("/one/two", await inspected.GetContentAsync());
    }

}
