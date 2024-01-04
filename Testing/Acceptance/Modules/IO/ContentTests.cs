using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ContentTests
    {

        [TestMethod]
        public async Task TestContent()
        {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Hello World!", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestContentIgnoresRouting()
        {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponseAsync("/some/path");

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

    }

}
