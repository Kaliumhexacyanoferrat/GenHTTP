using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Security;

[TestClass]
public sealed class ExtensionTests
{

    [TestMethod]
    public async Task ServerCanBeHardened()
    {
        using var runner = new TestHost(Layout.Create().Build());

        runner.Host.Handler(Content.From(Resource.FromString("Hello Eve!")))
              .Harden()
              .Start();

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("nosniff", response.GetHeader("X-Content-Type-Options"));
    }
}
