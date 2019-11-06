using System.Net;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;

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
                               .Add("index", Content.From("Hello World!"), true);

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hello World!", response.GetContent());
            }
        }

        /// <summary>
        /// As a developer I can set templates for pages to be rendered in.
        /// </summary>
        [Fact]
        public void TestWithTemplate()
        {
            var layout = Layout.Create()
                               .Template(ModScriban.Template(Data.FromString("{{ title }}{{ content }}")))
                               .Add("index", Page.From("Hello World!").Title("Hey there: "), true);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal("Hey there: Hello World!", response.GetContent());
        }

        /// <summary>
        /// As a developer I can set a default handler to be used for requests.
        /// </summary>
        [Fact]
        public void TestDefaultContent()
        {
            var layout = Layout.Create().Default(Content.From("Hello World!"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World!", response.GetContent());
        }

    }

}
