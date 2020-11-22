using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ContentTests
    {

        [TestMethod]
        public void TestContent()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", response.GetContent());
        }

        [TestMethod]
        public void TestContentIgnoresRouting()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse("/some/path");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

    }

}
