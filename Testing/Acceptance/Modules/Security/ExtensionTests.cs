using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;

namespace GenHTTP.Testing.Acceptance.Modules.Security;

[TestClass]
public sealed class ExtensionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task ServerCanBeHardened(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        await runner.Host.Handler(Content.From(Resource.FromString("Hello Eve!")))
              .Harden()
              .StartAsync();

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("nosniff", response.GetHeader("X-Content-Type-Options"));
    }
}
