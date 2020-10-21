using System;
using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Markdown;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    #region Supporting data structures

    public class CustomModel : PageModel
    {

        public string World => "World";

        public CustomModel(IRequest request, IHandler handler) : base(request, handler) { }

    }

    public class PathModel : PageModel
    {

        public WebPath Path => new PathBuilder("/test/1").Build();

        public PathModel(IRequest r, IHandler h) : base(r, h) { }

    }

    #endregion

    public class PageTests
    {
        
        [Fact]
        public void TestStringPage()
        {
            var layout = Page.From("Hello World!")
                             .Title("My Page")
                             .Description("My Description");

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.NotEqual("Hello World!", content);
            Assert.Contains("Hello World!", content);

            Assert.Equal("text/html", response.GetResponseHeader("Content-Type"));
        }

        [Fact]
        public void TestMarkdownPage()
        {
            var md = @"# Hello World!
```csharp
// code
```";

            var page = ModMarkdown.Page(Data.FromString(md))
                                  .Title("Markdown Page")
                                  .Description("A page rendered with markdown");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Contains("<h1 id=\"hello-world\">Hello World!</h1>", content);

            Assert.Contains("<pre><code class=\"language-csharp\">// code", content);
        }

        [Fact]
        public void TestRendering()
        {
            ModelProvider<CustomModel> modelProvider = (r, h) => new CustomModel(r, h);

            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Data.FromString("Hello {{ world }}!"), modelProvider).Title("1").Description("2"),
                ModRazor.Page(Data.FromString("Hello @Model.World!"), modelProvider).Title("1").Description("2"),
                Placeholders.Page(Data.FromString("Hello [World]!"), modelProvider).Title("1").Description("2")
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
        public void TestContentInfo()
        {
            var page = Page.From("Hello world!")
                           .Title("My Title")
                           .Description("My Description");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains("<title>My Title</title>", content);
            Assert.Contains("<meta name=\"description\" content=\"My Description\"/>", content);
        }

        [Fact]
        public void TestNoContentInfo()
        {
            var page = Page.From("Hello world!");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains("<title>Untitled Page</title>", content);
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

        [Fact]
        public void TestRoutingToPath()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Data.FromString("{{ route path }}"), (IRequest r, IHandler h) => new PathModel(r, h)),
                ModRazor.Page(Data.FromString("@Model.Route(Model.Path)"), (IRequest r, IHandler h) => new PathModel(r, h)),
            };

            foreach (var provider in providers)
            {
                var layout = Layout.Create()
                                   .Add("page", provider)
                                   .Add("test", Content.From("test"));
                
                using var runner = TestRunner.Run(layout);

                using var response = runner.GetResponse("/page");

                var content = response.GetContent();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("/test/1", content);
            }
        }

    }

}
