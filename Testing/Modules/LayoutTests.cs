using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using System;

namespace GenHTTP.Testing.Acceptance.Modules
{

    [TestClass]
    public sealed class LayoutTests
    {

        /// <summary>
        /// As a developer I can define the default route to be devlivered.
        /// </summary>
        [TestMethod]
        public async Task TestGetIndex()
        {
            var layout = Layout.Create()
                               .Index(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Hello World!", await response.GetContent());

            using var notFound = await runner.GetResponse("/notfound");

            await notFound.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// As a developer I can set a default handler to be used for requests.
        /// </summary>
        [TestMethod]
        public async Task TestDefaultContent()
        {
            var layout = Layout.Create().Add(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            foreach (var path in new string[] { "/something", "/" })
            {
                using var response = await runner.GetResponse(path);

                await response.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("Hello World!", await response.GetContent());
            }
        }

        /// <summary>
        /// As the developer of a web application, I don't want my application
        /// to produce duplicate content for missing trailing slashes.
        /// </summary>
        [TestMethod]
        public async Task TestRedirect()
        {
            var layout = Layout.Create()
                               .Add("section", Layout.Create().Index(Content.From(Resource.FromString("Hello World!"))));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/section/");

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Hello World!", await response.GetContent());

            using var redirected = await runner.GetResponse("/section");

            await redirected.AssertStatusAsync(HttpStatusCode.MovedPermanently);
            AssertX.EndsWith("/section/", redirected.GetHeader("Location")!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIllegalPathCharacters()
        {
            Layout.Create().Add("some/path", Content.From(Resource.FromString("Hello World")));
        }

    }

}
