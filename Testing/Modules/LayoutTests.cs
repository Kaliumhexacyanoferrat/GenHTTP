using Xunit;

using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules
{

    public class LayoutTests
    {

        /// <summary>
        /// As a developer I can define the default route to be devlivered.
        /// </summary>
        [Fact]
        public void TestGetIndex()
        {
            var layout = Layout.Create()
                               .Index(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World!", response.GetContent());

            using var notFound = runner.GetResponse("/notfound");

            Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
        }

        /// <summary>
        /// As a developer I can set a default handler to be used for requests.
        /// </summary>
        [Fact]
        public void TestDefaultContent()
        {
            var layout = Layout.Create().Fallback(Content.From(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(layout);

            foreach (var path in new string[] { "/something", "/" })
            {
                using var response = runner.GetResponse("/somethingelse");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hello World!", response.GetContent());
            }
        }

        /// <summary>
        /// As the developer of a web application, I don't want my application
        /// to produce duplicate content for missing trailing slashes.
        /// </summary>
        [Fact]
        public void TestRedirect()
        {
            var layout = Layout.Create()
                               .Add("section", Layout.Create().Index(Content.From(Resource.FromString("Hello World!"))));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/section/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World!", response.GetContent());

            using var redirected = runner.GetResponse("/section");

            Assert.Equal(HttpStatusCode.MovedPermanently, redirected.StatusCode);
            Assert.EndsWith("/section/", redirected.Headers["Location"]);
        }

    }

}
