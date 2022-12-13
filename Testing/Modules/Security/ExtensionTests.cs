using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

using GenHTTP.Modules.Security;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Security
{

    [TestClass]
    public sealed class ExtensionTests
    {

        [TestMethod]
        public async Task ServerCanBeHardened()
        {
            using var runner = new TestRunner();

            runner.Host.Handler(Content.From(Resource.FromString("Hello Eve!")))
                       .Harden()
                       .Start();

            using var response = await runner.GetResponse();

            Assert.AreEqual("nosniff", response.GetHeader("X-Content-Type-Options"));
        }

    }

}
