using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Scriban;
using GenHTTP.Modules.Websites;
using GenHTTP.Modules.Websites.Sites;

using Xunit;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class WebsiteTests
    {

        #region Supporting data structures

        public class Theme : ITheme
        {

            public List<Script> Scripts
            {
                get { return new List<Script> { new Script("custom.js", true, Resource.FromString(" ").Build()) }; }
            }

            public List<Style> Styles
            {
                get { return new List<Style> { new Style("custom.css", Resource.FromString(" ").Build()) }; }
            }

            public IHandlerBuilder? Resources => Layout.Create().Add("some.txt", Content.FromString("Text"));

            public IRenderer<ErrorModel> ErrorHandler => ModScriban.Template<ErrorModel>(Resource.FromAssembly("Error.html")).Build();

            public IRenderer<WebsiteModel> Renderer => ModScriban.Template<WebsiteModel>(Resource.FromAssembly("Template.html")).Build();

            public object? GetModel(IRequest request, IHandler handler)
            {
                return new { key = "value" };
            }

        }

        #endregion

        [Fact]
        public void TestErrorHandler()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/blubb");

            Assert.Equal(HttpStatusCode.NotFound, file.StatusCode);
            Assert.Equal("text/html", file.ContentType);

            var content = file.GetContent();

            Assert.Contains("This is an error!", content);

            Assert.Contains("This is the template!", content);
        }

        [Fact]
        public void TestDevelopmentResourcesWithoutBundle()
        {
            using var runner = new TestRunner();

            runner.Host.Handler(GetWebsite())
                       .Development(true)
                       .Start();

            using var style = runner.GetResponse("/styles/custom.css");
            Assert.Equal(HttpStatusCode.OK, style.StatusCode);

            using var script = runner.GetResponse("/scripts/custom.js");
            Assert.Equal(HttpStatusCode.OK, script.StatusCode);

            using var noStyle = runner.GetResponse("/styles/no.css");
            Assert.Equal(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = runner.GetResponse("/scripts/no.js");
            Assert.Equal(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [Fact]
        public void TestBundleNotServed()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var noStyle = runner.GetResponse("/styles/custom.css");
            Assert.Equal(HttpStatusCode.NotFound, noStyle.StatusCode);

            using var noScript = runner.GetResponse("/scripts/custom.js");
            Assert.Equal(HttpStatusCode.NotFound, noScript.StatusCode);
        }

        [Fact]
        public void TestCustomContent()
        {
            var website = GetWebsite().AddScript("my.js", Resource.FromString("my"))
                                      .AddStyle("my.css", Resource.FromString("my"));

            using var runner = TestRunner.Run(website);

            using var style = runner.GetResponse("/styles/bundle.css");
            Assert.Contains("my", style.GetContent());

            using var script = runner.GetResponse("/scripts/bundle.js");
            Assert.Contains("my", script.GetContent());
        }

        [Fact]
        public void TestStaticMenu()
        {
            var menu = Menu.Empty()
                           .Add("one", "One")
                           .Add("two", "Two", new List<(string, string)> { ("three", "Three") });

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestDynamicMenu()
        {
            var menu = Menu.From("{website}");

            var website = GetWebsite().Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestResources()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/resources/some.txt");
            Assert.Equal(HttpStatusCode.OK, file.StatusCode);

            using var noFile = runner.GetResponse("/resources/other.txt");
            Assert.Equal(HttpStatusCode.NotFound, noFile.StatusCode);
        }

        [Fact]
        public void TestFavicon()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/favicon.ico");
            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
            Assert.Equal("image/x-icon", file.ContentType);
        }

        [Fact]
        public void TestSitemap()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/sitemap.xml");

            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
            Assert.Equal("text/xml", file.ContentType);
        }

        [Fact]
        public void TestRobots()
        {
            using var runner = TestRunner.Run(GetWebsite());

            using var file = runner.GetResponse("/robots.txt");

            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
            Assert.Equal("text/plain", file.ContentType);
        }

        [Fact]
        public void TestCoreWebsiteWithoutResources()
        {
            using var runner = TestRunner.Run();

            using var robots = runner.GetResponse("/robots.txt");
            Assert.Equal(HttpStatusCode.NotFound, robots.StatusCode);

            using var favicon = runner.GetResponse("/favicon.ico");
            Assert.Equal(HttpStatusCode.NotFound, favicon.StatusCode);

            using var sitemap = runner.GetResponse("/sitemaps/sitemap.xml");
            Assert.Equal(HttpStatusCode.NotFound, sitemap.StatusCode);
        }

        [Fact]
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

            Assert.Contains("script = ../scripts/s.js", result);
            Assert.Contains("style = ../styles/s.css", result);
            Assert.Contains("resource = ../resources/r.txt", result);

            Assert.Contains("sitemap = ../sitemap.xml", result);
            Assert.Contains("favicon = ../favicon.ico", result);
            Assert.Contains("robots = ../robots.txt", result);

            Assert.Contains("root = ../", result);
            Assert.Contains("root-appended = ../my/file.txt", result);
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
