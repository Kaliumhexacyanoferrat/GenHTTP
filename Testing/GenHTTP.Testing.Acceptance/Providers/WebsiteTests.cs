using System;
using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Layouting;
using GenHTTP.Modules.Core.Websites;
using GenHTTP.Modules.Scriban;
using GenHTTP.Testing.Acceptance.Domain;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class WebsiteTests
    {

        #region Supporting data structures

        public class Theme : ITheme
        {
            public List<Script> Scripts
            {
                get { return new List<Script> { new Script("custom.js", true, Data.FromString(" ").Build()) }; }
            }

            public List<Style> Styles
            {
                get { return new List<Style> { new Style("custom.css", Data.FromString(" ").Build()) }; }
            }

            public IRouter? Resources => Layout.Create().Add("some.txt", Content.From("Text")).Build();

            public IContentProvider? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
            {
                return Content.From("Error!").Build();
            }

            public object? GetModel(IRequest request)
            {
                return new { key = "value" };
            }

            public IRenderer<WebsiteModel> GetRenderer()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        [Fact]
        public void TestDevelopmentResourcesWithoutBundle()
        {
            using var runner = new TestRunner();

            runner.Host.Router(GetWebsite())
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
            var website = GetWebsite().AddScript("my.js", Data.FromString("my"))
                                      .AddStyle("my.css", Data.FromString("my"));

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
            var index = response.GetContent();
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

            using var file = runner.GetResponse("/sitemaps/sitemap.xml");

            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
            Assert.Equal("text/xml", file.ContentType);
        }

        [Fact]
        public void TestWebsiteContent()
        {
            var outer = Website.Create()
                               .Theme(new Theme())
                               .Content(GetWebsite());

            using var runner = TestRunner.Run(outer);

            using var file = runner.GetResponse("/sitemaps/sitemap.xml");

            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
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
                             sitemap = {{ route 'sitemaps/s.xml' }}
                             favicon = {{ route 'favicon.ico' }}
                             robots = {{ route 'robots.txt' }}
                             root = {{ route '{root}' }}
                             root-appended = {{ route '{root}/my/file.txt' }}
                             else = {{ route 'something/else/' }}";

            var sub = Layout.Create()
                            .Add("index", ModScriban.Page(Data.FromString(template)), true);

            var content = Layout.Create()
                                .Add("sub", sub);

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse("/sub/");

            var result = response.GetContent();

            Assert.Contains("script = ../scripts/s.js", result);
            Assert.Contains("style = ../styles/s.css", result);
            Assert.Contains("resource = ../resources/r.txt", result);

            Assert.Contains("sitemap = ../sitemaps/s.xml", result);
            Assert.Contains("favicon = ../favicon.ico", result);
            Assert.Contains("robots = ../robots.txt", result);

            Assert.Contains("root = ../", result);
            Assert.Contains("root-appended = ../my/file.txt", result);
        }

        private WebsiteBuilder GetWebsite(LayoutBuilder? content = null)
        {
            return Website.Create()
                          .Theme(new Theme())
                          .Content(content ?? Layout.Create())
                          .Favicon(Data.FromString("This is a favicon"));
        }

    }

}
