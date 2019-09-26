using Xunit;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;
using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class PageTests
    {

        private class CustomModel : PageModel
        {

            public string World => "World";

            public CustomModel(IRequest request) : base(request) { }

        }

        /// <summary>
        /// As a developer, I can provide pages with plain text.
        /// </summary>
        [Fact]
        public void TestStringPage()
        {
            var layout = Layout.Create().Add("page", Page.From("Hello World!"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/page");

            var content = response.GetContent();

            Assert.NotEqual("Hello World!", content);
            Assert.Contains("Hello World!", content);

            Assert.Equal("text/html", response.GetResponseHeader("Content-Type"));
        }

        /// <summary>
        /// As a developer, I can provide pages with placeholders.
        /// </summary>
        [Fact]
        public void TestPlaceholderPage()
        {
            var page = Data.FromString("Hello [World]!");
            var layout = Layout.Create().Add("page", Placeholders.Page(page, (r) => new CustomModel(r)));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/page");

            var content = response.GetContent();

            Assert.NotEqual("Hello World!", content);
            Assert.Contains("Hello World!", content);
        }

        /// <summary>
        /// As a developer, I can provide pages with Scriban templates.
        /// </summary>
        [Fact]
        public void TestScribanPage()
        {
            var page = Data.FromString("Hello {{ world }}!");
            var layout = Layout.Create().Add("page", ModScriban.Page(page, (r) => new CustomModel(r)));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/page");

            var content = response.GetContent();

            Assert.NotEqual("Hello World!", content);
            Assert.Contains("Hello World!", content);
        }

        /// <summary>
        /// As a developer, I can generate links in Scriban pages.
        /// </summary>
        [Fact]
        public void TestScribanPageRouting()
        {
            var page = Data.FromString("{{ route 'https://google.de' }}|{{ route 'res/123' }}|{{ route 'other/456/' }}|{{ route './relative' }}");

            var inner = Layout.Create()
                               .Add("page", ModScriban.Page(page));

            var outer = Layout.Create()
                              .Add("res", Layout.Create())
                              .Add("inner", inner);

            var layout = Layout.Create()
                               .Add("other", Layout.Create())
                               .Add("outer", outer);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/outer/inner/page");

            var content = response.GetContent();

            Assert.Contains("https://google.de|../res/123|../../other/456/|./relative", content);
        }

    }

}
