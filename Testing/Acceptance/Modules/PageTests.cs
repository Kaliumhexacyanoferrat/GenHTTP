using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Markdown;
using GenHTTP.Modules.Pages;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Providers
{

    #region Supporting data structures

    public sealed class CustomModel : AbstractModel
    {

        public string World => "World";

        public CustomModel(IRequest request, IHandler handler) : base(request, handler) { }

        public override ValueTask<ulong> CalculateChecksumAsync() => new(17);

    }

    public sealed class PathModel : AbstractModel
    {

        public WebPath Path => new PathBuilder("/test/1").Build();

        public PathModel(IRequest r, IHandler h) : base(r, h) { }

        public override ValueTask<ulong> CalculateChecksumAsync() => new(17);

    }

    #endregion

    [TestClass]
    public sealed class PageTests
    {
        
        [TestMethod]
        public async Task TestStringPage()
        {
            var layout = Page.From("Hello World!")
                             .Title("My Page")
                             .Description("My Description");

            using var runner = TestHost.Run(layout);

            using var response = await runner.GetResponseAsync();

            var content = await response.GetContentAsync();

            Assert.AreNotEqual("Hello World!", content);
            AssertX.Contains("Hello World!", content);

            Assert.AreEqual("text/html", response.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestMarkdownPage()
        {
            var md = @"# Hello World!
```csharp
// code
```";

            var page = ModMarkdown.Page(Resource.FromString(md))
                                  .Title("Markdown Page")
                                  .Description("A page rendered with markdown");

            using var runner = TestHost.Run(page);

            using var response = await runner.GetResponseAsync();

            var content = await response.GetContentAsync();

            AssertX.Contains("<h1 id=\"hello-world\">Hello World!</h1>", content);

            AssertX.Contains("<pre><code class=\"language-csharp\">// code", content);
        }

        [TestMethod]
        public async Task TestRendering()
        {
            static ValueTask<CustomModel> modelProvider(IRequest r, IHandler h) => new ValueTask<CustomModel>(new CustomModel(r, h));

            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Resource.FromString("Hello {{ world }}!"), modelProvider).Title("1").Description("2"),
                ModRazor.Page(Resource.FromString("Hello @Model.World!"), modelProvider).Title("1").Description("2"),
                Placeholders.Page(Resource.FromString("Hello [World]!"), modelProvider).Title("1").Description("2")
            };

            foreach (var provider in providers)
            {
                var layout = Layout.Create().Add("page", provider);

                using var runner = TestHost.Run(layout);

                using var response = await runner.GetResponseAsync("/page");

                var content = await response.GetContentAsync();

                Assert.AreNotEqual("Hello World!", content);
                AssertX.Contains("Hello World!", content);
            }
        }

        [TestMethod]
        public async Task TestContentInfo()
        {
            var page = Page.From("Hello world!")
                           .Title("My Title")
                           .Description("My Description");

            using var runner = TestHost.Run(page);

            using var response = await runner.GetResponseAsync();

            var content = await response.GetContentAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);

            AssertX.Contains("<title>My Title</title>", content);
            AssertX.Contains("<meta name=\"description\" content=\"My Description\"/>", content);
        }

        [TestMethod]
        public async Task TestNoContentInfo()
        {
            var page = Page.From("Hello world!");

            using var runner = TestHost.Run(page);

            using var response = await runner.GetResponseAsync();

            var content = await response.GetContentAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);

            AssertX.Contains("<title>Untitled Page</title>", content);
            AssertX.Contains("<meta name=\"description\" content=\"\"/>", content);
        }

        [TestMethod]
        public async Task TestRouting()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Resource.FromString("{{ route 'https://google.de' }}|{{ route 'res/123' }}|{{ route 'other/456/' }}|{{ route './relative' }}")),
                ModRazor.Page(Resource.FromString("@Model.Route(\"https://google.de\")|@Model.Route(\"res/123\")|@Model.Route(\"other/456/\")|@Model.Route(\"./relative\")")),
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

                using var runner = TestHost.Run(layout);

                using var response = await runner.GetResponseAsync("/outer/inner/page");

                await response.AssertStatusAsync(HttpStatusCode.OK);

                var content = await response.GetContentAsync();

                AssertX.Contains("https://google.de|../res/123|../../other/456/|./relative", content);
            }
        }

        [TestMethod]
        public async Task TestRoutingToPath()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page<PathModel>(Resource.FromString("{{ route path }}"), (IRequest r, IHandler h) => new(new PathModel(r, h))),
                ModRazor.Page(Resource.FromString("@Model.Route(Model.Path)"), (IRequest r, IHandler h) => new PathModel(r, h)),
            };

            foreach (var provider in providers)
            {
                var layout = Layout.Create()
                                   .Add("page", provider)
                                   .Add("test", Content.From(Resource.FromString("test")));
                
                using var runner = TestHost.Run(layout);

                using var response = await runner.GetResponseAsync("/page");

                var content = await response.GetContentAsync();

                await response.AssertStatusAsync(HttpStatusCode.OK);
                AssertX.Contains("/test/1", content);
            }
        }

        [TestMethod]
        public async Task TestAdditions()
        {
            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Resource.FromString("Scriban")).AddScript("myscript.js").AddStyle("mystyle.css"),
                ModRazor.Page(Resource.FromString("Razor")).AddScript("./myscript.js").AddStyle("./mystyle.css"),
                Placeholders.Page(Resource.FromString("Placeholders")).AddScript("http://myserver/myscript.js").AddStyle("http://myserver/mystyle.css"),
                CombinedPage.Create().AddMarkdown(Resource.FromString("Scriban")).AddScript("myscript.js").AddStyle("mystyle.css"),
                ModMarkdown.Page(Resource.FromString("Markdown")).AddScript("myscript.js").AddStyle("mystyle.css")
            };

            foreach (var provider in providers)
            {
                using var runner = TestHost.Run(provider);

                using var response = await runner.GetResponseAsync();

                var content = await response.GetContentAsync();

                AssertX.Contains("myscript.js", content);
                AssertX.Contains("mystyle.css", content);
            }
        }

        [TestMethod]
        public async Task TestModifications()
        {
            var cookie = new Api.Protocol.Cookie("k", "v");

            var providers = new List<IHandlerBuilder>()
            {
                ModScriban.Page(Resource.FromString("Scriban")).Status(ResponseStatus.Locked).Header("H", "V").Expires(DateTime.Now).Modified(DateTime.Now).Type(new(ContentType.FontOpenTypeFont)).Encoding("my-encoding").Cookie(cookie),
                ModRazor.Page(Resource.FromString("Razor")).Status(ResponseStatus.Locked).Header("H", "V").Expires(DateTime.Now).Modified(DateTime.Now).Type(new(ContentType.FontOpenTypeFont)).Encoding("my-encoding").Cookie(cookie),
                Placeholders.Page(Resource.FromString("Placeholders")).Status(ResponseStatus.Locked).Header("H", "V").Expires(DateTime.Now).Modified(DateTime.Now).Type(new(ContentType.FontOpenTypeFont)).Encoding("my-encoding").Cookie(cookie),
                ModMarkdown.Page(Resource.FromString("Markdown")).Status(ResponseStatus.Locked).Header("H", "V").Expires(DateTime.Now).Modified(DateTime.Now).Type(new(ContentType.FontOpenTypeFont)).Encoding("my-encoding").Cookie(cookie),
                CombinedPage.Create().Add("Combined").Status(ResponseStatus.Locked).Header("H", "V").Expires(DateTime.Now).Modified(DateTime.Now).Type(new(ContentType.FontOpenTypeFont)).Encoding("my-encoding").Cookie(cookie)
            };

            foreach (var provider in providers)
            {
                var cookies = new CookieContainer();

                using var runner = TestHost.Run(provider);

                using var response = await runner.GetResponseAsync(client: TestHost.GetClient(cookies: cookies));

                var content = await response.GetContentAsync();

                await response.AssertStatusAsync(HttpStatusCode.Locked);

                Assert.AreEqual("V", response.GetHeader("H"));

                Assert.IsNotNull(response.Content.Headers.Expires);
                Assert.IsNotNull(response.Content.Headers.LastModified);

                Assert.AreEqual("font/otf", response.GetContentHeader("Content-Type"));
                Assert.AreEqual("my-encoding", response.GetContentHeader("Content-Encoding"));

                Assert.AreEqual(1, cookies.Count);
            }
        }

    }

}
