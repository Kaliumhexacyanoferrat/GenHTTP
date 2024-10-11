using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

using GenHTTP.Modules.Security;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Security;

[TestClass]
public sealed class ExtensionTests
{

    [TestMethod]
    public async Task ServerCanBeHardened()
    {
            using var runner = new TestHost(Layout.Create());

            runner.Host.Handler(Content.From(Resource.FromString("Hello Eve!")))
                       .Harden()
                       .Start();

            using var response = await runner.GetResponseAsync();

            Assert.AreEqual("nosniff", response.GetHeader("X-Content-Type-Options"));
        }

}
