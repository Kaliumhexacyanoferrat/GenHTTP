using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public class ContentTests
    {

        #region Supporting data structures

        public class TestController
        {
            private static readonly IHandlerBuilder _Page = Page.From(string.Empty);

            public IHandlerBuilder Index() => _Page;

            public IHandlerBuilder Complex() => Layout.Create().Add("page", _Page);

            public IHandler WithParent(IHandler parent, IRequest _) => _Page.Build(parent);

            public void Ignored() { }

            [ControllerAction(RequestMethod.GET, IgnoreContent = true)]
            public IHandlerBuilder IgnoredContent() => _Page;

            [ControllerAction(RequestMethod.POST)]
            public IHandlerBuilder Post(string _) => _Page;

            public IHandlerBuilder NoHints([FromPath] string path) => _Page;

            [ControllerAction(RequestMethod.GET, ContentHints = typeof(Hints))]
            public IHandlerBuilder WithHints([FromPath] int number, int query) => _Page;

        }

        public class Hints : IContentHints
        {
            public List<ContentHint> GetHints(IRequest request)
            {
                return new List<ContentHint>()
                {
                    new ContentHint() { { "number", 10 }, { "query", 11 } },
                    new ContentHint() { { "number", 12 } }
                };
            }
        }

        #endregion

        #region Tests

        [TestMethod]
        public void TestIndex() => AssertX.Contains("/c/", GetSitemap());

        [TestMethod]
        public void TestComplex() => AssertX.Contains("/c/complex/page", GetSitemap());

        [TestMethod]
        public void TestWithParent() => AssertX.Contains("/c/with-parent/", GetSitemap());

        [TestMethod]
        public void TestOthersIgnored() => AssertX.DoesNotContain("/c/ignored/", GetSitemap());

        [TestMethod]
        public void TestIgnoredContent() => AssertX.DoesNotContain("/c/ignored-content/", GetSitemap());

        [TestMethod]
        public void TestPostIsIgnored() => AssertX.DoesNotContain("/c/post/", GetSitemap());
        
        [TestMethod]
        public void TestNoHintsNoContent() => AssertX.DoesNotContain("/c/no-hints/", GetSitemap());

        [TestMethod]
        public void TestHints()
        {
            var sitemap = GetSitemap();

            AssertX.Contains("/c/with-hints/10/", sitemap);
            AssertX.Contains("/c/with-hints/12/", sitemap);
        }

        #endregion

        #region Helpers

        private HashSet<string> GetSitemap()
        {
            var root = Layout.Create()
                             .AddController<TestController>("c")
                             .Add("sitemap", Sitemap.Create());

            using var runner = TestRunner.Run(root);

            using var response = runner.GetResponse("/sitemap");

            return response.GetSitemap();
        }

        #endregion

    }

}
