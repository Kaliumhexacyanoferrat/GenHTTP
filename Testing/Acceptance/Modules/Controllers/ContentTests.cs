using System.Collections.Generic;
using System.Threading.Tasks;

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
    public sealed class ContentTests
    {

        #region Supporting data structures

        public sealed class TestController
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

        public sealed class Hints : IContentHints
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
        public async Task TestIndex() => AssertX.Contains("/c/", await GetSitemap());

        [TestMethod]
        public async Task TestComplex() => AssertX.Contains("/c/complex/page", await GetSitemap());

        [TestMethod]
        public async Task TestWithParent() => AssertX.Contains("/c/with-parent/", await GetSitemap());

        [TestMethod]
        public async Task TestOthersIgnored() => AssertX.DoesNotContain("/c/ignored/", await GetSitemap());

        [TestMethod]
        public async Task TestIgnoredContent() => AssertX.DoesNotContain("/c/ignored-content/", await GetSitemap());

        [TestMethod]
        public async Task TestPostIsIgnored() => AssertX.DoesNotContain("/c/post/", await GetSitemap());
        
        [TestMethod]
        public async Task TestNoHintsNoContent() => AssertX.DoesNotContain("/c/no-hints/", await GetSitemap());

        [TestMethod]
        public async Task TestHints()
        {
            var sitemap = await GetSitemap();

            AssertX.Contains("/c/with-hints/10/", sitemap);
            AssertX.Contains("/c/with-hints/12/", sitemap);
        }

        #endregion

        #region Helpers

        private async Task<HashSet<string>> GetSitemap()
        {
            var root = Layout.Create()
                             .AddController<TestController>("c")
                             .Add("sitemap", Sitemap.Create());

            using var runner = TestHost.Run(root);

            using var response = await runner.GetResponseAsync("/sitemap");

            return await response.GetSitemap();
        }

        #endregion

    }

}
