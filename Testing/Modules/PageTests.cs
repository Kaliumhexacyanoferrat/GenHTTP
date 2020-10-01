using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;
using System.Collections.Generic;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    #region Supporting data structures

    public class CustomModel : PageModel
    {

        public string World => "World";

        public CustomModel(IRequest request, IHandler handler) : base(request, handler) { }

    }

    #endregion

    public class PageTests
    {

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

        [Fact]
        public void TestRendering()
        {
            ModelProvider<CustomModel> modelProvider = (r, h) => new CustomModel(r, h);

            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Data.FromString("Hello {{ world }}!"), modelProvider),
                ModRazor.Page(Data.FromString("Hello @Model.World!"), modelProvider),
                Placeholders.Page(Data.FromString("Hello [World]!"), modelProvider)
            };

            foreach (var provider in providers)
            {
                var layout = Layout.Create().Add("page", provider);

                using var runner = TestRunner.Run(layout);

                using var response = runner.GetResponse("/page");

                var content = response.GetContent();

                Assert.NotEqual("Hello World!", content);
                Assert.Contains("Hello World!", content);
            }
        }

        [Fact]
        public void TestDescription()
        {
            var page = Page.From("Hello world!")
                           .Title("My Title")
                           .Description("My Description");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Contains("<meta name=\"description\" content=\"My Description\"/>", content);
        }

        [Fact]
        public void TestNoDescription()
        {
            var page = Page.From("Hello world!")
                           .Title("My Title");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Contains("<meta name=\"description\" content=\"\"/>", content);
        }

        [Fact]
        public void TestRouting()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Data.FromString("{{ route 'https://google.de' }}|{{ route 'res/123' }}|{{ route 'other/456/' }}|{{ route './relative' }}")),
                ModRazor.Page(Data.FromString("@Model.Route(\"https://google.de\")|@Model.Route(\"res/123\")|@Model.Route(\"other/456/\")|@Model.Route(\"./relative\")")),
            };

            foreach (var provider in providers)
            {
                var inner = Layout.Create()
                                  .Add("page", provider);

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

}
