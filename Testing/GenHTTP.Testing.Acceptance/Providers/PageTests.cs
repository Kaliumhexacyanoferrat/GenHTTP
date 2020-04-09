using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;

using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Providers
{

    #region Supporting data structures

    public class CustomModel : PageModel
    {

        public string World => "World";

        public CustomModel(IRequest request) : base(request) { }

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
            ModelProvider<CustomModel> modelProvider = (r) => new CustomModel(r);

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
        public void TestRouting()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Data.FromString("{{ route 'https://google.de' }}|{{ route 'res/123' }}|{{ route 'other/456/' }}|{{ route './relative' }}")),
                ModRazor.Page(Data.FromString("@Model.Request.Routing.Route(\"https://google.de\")|@Model.Request.Routing.Route(\"res/123\")|@Model.Request.Routing.Route(\"other/456/\")|@Model.Request.Routing.Route(\"./relative\")")),
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
