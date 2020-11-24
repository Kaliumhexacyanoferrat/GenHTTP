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
        public void TestErrorHandler()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/blubb");

            Assert.AreEqual(HttpStatusCode.NotFound, file.StatusCode);
            Assert.AreEqual("text/html", file.ContentType);

            var content = file.GetContent();

            AssertX.Contains("This is an error!", content);

            AssertX.Contains("This is the template!", content);
        }

        [TestMethod]
        public void TestDevelopmentResourcesWithoutBundle()
        {
            using var runner = new TestRunner();

            runner.Host.Handler(GetWebsite())
                       .Development(true)
                       .Start();

            using var style = runner.GetResponse("/styles/custom.css");
            Assert.AreEqual(HttpStatusCode.OK, style.StatusCode);

            using var script = runner.GetResponse("/scripts/custom.js");
            Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);

            using var noStyle = runner.GetResponse("/styles/no.css");
            Assert.AreEqual(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = runner.GetResponse("/scripts/no.js");
            Assert.AreEqual(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [TestMethod]
        public void TestBundleNotServed()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var noStyle = runner.GetResponse("/styles/custom.css");
            Assert.AreEqual(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = runner.GetResponse("/scripts/custom.js");
            Assert.AreEqual(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [TestMethod]
        public void TestCustomContent()
        {
            var website = GetWebsite().AddScript("my.js", Resource.FromString("my"))
                                      .AddStyle("my.css", Resource.FromString("my"));

            using var runner = TestRunner.Run(website);

            using var style = runner.GetResponse("/styles/bundle.css");
            AssertX.Contains("my", style.GetContent());

            using var script = runner.GetResponse("/scripts/bundle.js");
            AssertX.Contains("my", script.GetContent());
        }

        [TestMethod]
        public void TestStaticMenu()
        {
            var menu = Menu.Empty()
                           .Add("one", "One")
                           .Add("two", "Two", new List<(string, string)> { ("three", "Three") });

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestDynamicMenu()
        {
            var menu = Menu.From("{website}");

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestResources()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/resources/some.txt");
            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);

            using var noFile = runner.GetResponse("/resources/other.txt");
            Assert.AreEqual(HttpStatusCode.NotFound, noFile.StatusCode);
        }

        [TestMethod]
        public void TestFavicon()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/favicon.ico");
            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("image/x-icon", file.ContentType);
        }

        [TestMethod]
        public void TestSitemap()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/sitemap.xml");

            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("text/xml", file.ContentType);
        }

        [TestMethod]
        public void TestRobots()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/robots.txt");

            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
            Assert.AreEqual("text/plain", file.ContentType);
        }

        [TestMethod]
        public void TestCoreWebsiteWithoutResources()
        {
            using var runner = TestRunner.Run();

            using var robots = runner.GetResponse("/robots.txt");
            Assert.AreEqual(HttpStatusCode.NotFound, robots.StatusCode);

            using var favicon = runner.GetResponse("/favicon.ico");
            Assert.AreEqual(HttpStatusCode.NotFound, favicon.StatusCode);

            using var sitemap = runner.GetResponse("/sitemaps/sitemap.xml");
            Assert.AreEqual(HttpStatusCode.NotFound, sitemap.StatusCode);
        }

        [TestMethod]
        public void TestWebsiteRouting()
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

            using var response = runner.GetResponse("/sub/");

            var result = response.GetContent();

            AssertX.Contains("script = ../scripts/s.js", result);
            AssertX.Contains("style = ../styles/s.css", result);
            AssertX.Contains("resource = ../resources/r.txt", result);

            AssertX.Contains("sitemap = ../sitemap.xml", result);
            AssertX.Contains("favicon = ../favicon.ico", result);
            AssertX.Contains("robots = ../robots.txt", result);

            AssertX.Contains("root = ../", result);
            AssertX.Contains("root-appended = ../my/file.txt", result);
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
