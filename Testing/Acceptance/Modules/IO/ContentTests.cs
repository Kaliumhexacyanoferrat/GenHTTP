using System.Net;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class ContentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContent(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentIgnoresRouting(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        using var response = await runner.GetResponseAsync("/some/path");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }
}
