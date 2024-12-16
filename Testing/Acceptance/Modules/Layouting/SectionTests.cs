using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Layouting;

[TestClass]
public class SectionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSection(TestEngine engine)
    {
        var app = Layout.Create();

        var section = app.AddSection("section");

        section.Index(Content.From(Resource.FromString("Hello World")));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/section/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

}
