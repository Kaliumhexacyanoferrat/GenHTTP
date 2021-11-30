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
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestContentIgnoresRouting()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponse("/some/path");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

    }

}
