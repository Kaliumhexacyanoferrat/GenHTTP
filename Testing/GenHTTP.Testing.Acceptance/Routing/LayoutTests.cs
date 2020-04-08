using Xunit;

using System.Net;

using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Routing
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
                               .Index(Content.From("Hello World!"));

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
            var layout = Layout.Create().Fallback(Content.From("Hello World!"));

            using var runner = TestRunner.Run(layout);

            foreach (var path in new string[] { "/something", "/" })
            {
                using var response = runner.GetResponse("/somethingelse");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hello World!", response.GetContent());
            }
        }

    }

}
