using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Scriban;
using GenHTTP.Modules.Websites;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.AutoReload;
using GenHTTP.Modules.Websites.Sites;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class WebsiteTests
    {

        #region Supporting data structures

        public sealed class Theme : ITheme
        {

            public List<Script> Scripts
            {
                get { return new List<Script> { new Script("custom.js", true, Resource.FromString(" ").Build()) }; }
            }

            public List<Style> Styles
            {
                get { return new List<Style> { new Style("custom.css", Resource.FromString(" ").Build()) }; }
            }

            public IHandlerBuilder? Resources => Layout.Create().Add("some.txt", Content.From(Resource.FromString("Text")));

            public IRenderer<ErrorModel> ErrorHandler => ModScriban.Template<ErrorModel>(Resource.FromAssembly("Error.html")).Build();

            public IRenderer<WebsiteModel> Renderer => ModScriban.Template<WebsiteModel>(Resource.FromAssembly("Template.html")).Build();

            public ValueTask<object?> GetModelAsync(IRequest request, IHandler handler)
            {
                return ValueTask.FromResult<object?>(new { key = "value" });
            }

        }

        #endregion

        [TestMethod]
        public async Task TestErrorHandler()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = await runner.GetResponse("/blubb");

            Assert.AreEqual(HttpStatusCode.NotFound, file.StatusCode);
            Assert.AreEqual("text/html; charset=UTF-8", file.GetContentHeader("Content-Type"));

            var content = await file.GetContent();

            AssertX.Contains("This is an error!", content);

            AssertX.Contains("This is the template!", content);
        }

        [TestMethod]
        public async Task TestDevelopmentResourcesWithoutBundle()
        {
            using var runner = new TestRunner();

            runner.Host.Handler(GetWebsite())
                       .Development(true)
                       .Start();

            using var style = await runner.GetResponse("/styles/custom.css");
            Assert.AreEqual(HttpStatusCode.OK, style.StatusCode);

            using var script = await runner.GetResponse("/scripts/custom.js");
            Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);

            using var noStyle = await runner.GetResponse("/styles/no.css");
            Assert.AreEqual(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = await runner.GetResponse("/scripts/no.js");
            Assert.AreEqual(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [TestMethod]
        public async Task TestBundleNotServed()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var noStyle = await runner.GetResponse("/styles/custom.css");
            Assert.AreEqual(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = await runner.GetResponse("/scripts/custom.js");
            Assert.AreEqual(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [TestMethod]
        public async Task TestCustomContent()
        {
            var website = GetWebsite().AddScript("my.js", Resource.FromString("my"))
                                      .AddStyle("my.css", Resource.FromString("my"));

            using var runner = TestRunner.Run(website);

            using var style = await runner.GetResponse("/styles/bundle.css");
            AssertX.Contains("my", await style.GetContent());

            using var script = await runner.GetResponse("/scripts/bundle.js");
            AssertX.Contains("my", await script.GetContent());
        }

        [TestMethod]
        public async Task TestStaticMenu()
        {
            var menu = Menu.Empty()
                           .Add("one", "One")
                           .Add("two", "Two", new List<(string, string)> { ("three", "Three") });

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestDynamicMenu()
        {
            var menu = Menu.From("{website}");

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestResources()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = await runner.GetResponse("/resources/some.txt");
            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);

            using var noFile = await runner.GetResponse("/resources/other.txt");
            Assert.AreEqual(HttpStatusCode.NotFound, noFile.StatusCode);
        }

        [TestMethod]
        public async Task TestFavicon()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = await runner.GetResponse("/favicon.ico");
            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("image/x-icon", file.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestSitemap()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = await runner.GetResponse("/" + Sitemap.FILE_NAME);

            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("text/xml", file.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestRobots()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = await runner.GetResponse("/" + BotInstructions.FILE_NAME);

            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("text/plain", file.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestCoreWebsiteWithoutResources()
        {
            using var runner = TestRunner.Run();

            using var robots = await runner.GetResponse("/" + BotInstructions.FILE_NAME);
            Assert.AreEqual(HttpStatusCode.NotFound, robots.StatusCode);

            using var favicon = await runner.GetResponse("/favicon.ico");
            Assert.AreEqual(HttpStatusCode.NotFound, favicon.StatusCode);

            using var sitemap = await runner.GetResponse("/sitemaps/" + Sitemap.FILE_NAME);
            Assert.AreEqual(HttpStatusCode.NotFound, sitemap.StatusCode);
        }

        [TestMethod]
        public async Task TestWebsiteRouting()
        {
            var template = @"script = {{ route 'scripts/s.js' }}
                             style = {{ route 'styles/s.css' }}
                             resource = {{ route 'resources/r.txt' }}
                             sitemap = {{ route 'sitemap.xml' }}
                             favicon = {{ route 'favicon.ico' }}
                             robots = {{ route 'robots.txt' }}
                             root = {{ route '{website}' }}
                             root-appended = {{ route '{website}/my/file.txt' }}
                             else = {{ route 'something/else/' }}";

            var sub = Layout.Create()
                            .Index(ModScriban.Page(Resource.FromString(template)));

            var content = Layout.Create()
                                .Add("sub", sub);

            using var runner = TestRunner.Run(GetWebsite(content));

            using var response = await runner.GetResponse("/sub/");

            var result = await response.GetContent();

            AssertX.Contains("script = ../scripts/s.js", result);
            AssertX.Contains("style = ../styles/s.css", result);
            AssertX.Contains("resource = ../resources/r.txt", result);

            AssertX.Contains("sitemap = ../sitemap.xml", result);
            AssertX.Contains("favicon = ../favicon.ico", result);
            AssertX.Contains("robots = ../robots.txt", result);

            AssertX.Contains("root = ../", result);
            AssertX.Contains("root-appended = ../my/file.txt", result);
        }

        [TestMethod]
        public async Task TestSamePageSameChecksum()
        {
            var content = Layout.Create()
                                .Index(ModScriban.Page(Resource.FromString("Hello World!")));

            using var runner = TestRunner.Run(GetWebsite(content));

            using var resp1 = await runner.GetResponse();
            using var resp2 = await runner.GetResponse();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

        [TestMethod]
        public async Task TestAutoReload()
        {
            var website = GetWebsite().AutoReload();

            using var runner = new TestRunner();

            runner.Host.Handler(website)
                       .Development(true)
                       .Start();

            using var script = await runner.GetResponse("/scripts/genhttp-modules-autoreload.js");

            Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);

            AssertX.Contains("checkForModifications", await script.GetContent());
        }

        public static WebsiteBuilder GetWebsite(LayoutBuilder? content = null)
        {
            return Website.Create()
                          .Theme(new Theme())
                          .Content(content ?? Layout.Create())
                          .Favicon(Resource.FromString("This is a favicon").Type(ContentType.ImageIcon));
        }

    }

}
