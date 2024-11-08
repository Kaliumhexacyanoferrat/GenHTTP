using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class EncodingTests
{

    /// <summary>
    /// As a developer, I want UTF-8 to be my default encoding.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestUtf8DefaultEncoding(TestEngine engine)
    {
        var layout = Layout.Create().Add("utf8", Content.From(Resource.FromString("From GenHTTP with ❤")));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/utf8");

        Assert.AreEqual("From GenHTTP with ❤", await response.GetContentAsync());
    }

}
