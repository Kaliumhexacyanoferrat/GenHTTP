using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Websites;
using GenHTTP.Modules.Themes.Lorahost;
using GenHTTP.Testing.Acceptance.Domain;
using System;
using System.Collections.Generic;
using System.Net;
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

            var website = GetWebsite().Theme(new LorahostBuilder())
                                      .Menu(menu);

            using var runner = TestRunner.Run(website);

            using var response = runner.GetResponse();
            var index = response.GetContent();

            Assert.Contains("One", index);
            Assert.Contains("Two", index);
            Assert.Contains("Three", index);
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

        private WebsiteBuilder GetWebsite()
        {
            var content = Layout.Create();

            return Website.Create()
                          .Theme(new Theme())
                          .Content(content);
        }

    }

}
