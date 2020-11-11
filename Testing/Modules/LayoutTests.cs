using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules
{

    [TestClass]
    public class LayoutTests
    {

        /// <summary>
        /// As a developer I can define the default route to be devlivered.
        /// </summary>
        [TestMethod]
        public void TestGetIndex()
        {
            var layout = Layout.Create()
                               .Index(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", response.GetContent());

            using var notFound = runner.GetResponse("/notfound");

            Assert.AreEqual(HttpStatusCode.NotFound, notFound.StatusCode);
        }

        /// <summary>
        /// As a developer I can set a default handler to be used for requests.
        /// </summary>
        [TestMethod]
        public void TestDefaultContent()
        {
            var layout = Layout.Create().Fallback(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            foreach (var path in new string[] { "/something", "/" })
            {
                using var response = runner.GetResponse("/somethingelse");

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Hello World!", response.GetContent());
            }
        }

        /// <summary>
        /// As the developer of a web application, I don't want my application
        /// to produce duplicate content for missing trailing slashes.
        /// </summary>
        [TestMethod]
        public void TestRedirect()
        {
            var layout = Layout.Create()
                               .Add("section", Layout.Create().Index(Content.From(Resource.FromString("Hello World!"))));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/section/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", response.GetContent());

            using var redirected = runner.GetResponse("/section");

            Assert.AreEqual(HttpStatusCode.MovedPermanently, redirected.StatusCode);
            AssertX.EndsWith("/section/", redirected.Headers["Location"]!);
        }

    }

}
