using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class ChecksumTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSameErrorSameChecksum(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var resp1 = await runner.GetResponseAsync();
        using var resp2 = await runner.GetResponseAsync();

        Assert.IsNotNull(resp1.GetETag());

        Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSameContentSameChecksum(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        using var resp1 = await runner.GetResponseAsync();
        using var resp2 = await runner.GetResponseAsync();

        Assert.IsNotNull(resp1.GetETag());

        Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
    }

}
