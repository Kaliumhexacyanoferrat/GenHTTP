using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Sitemaps;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class SitemapTests
    {

        #region Helping data structures

        [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
        public sealed class UrlSet
        {

            [XmlElement("url")]
            public List<Url>? Entries { get; set; }

        }

        public sealed class Url
        {

            [XmlElement("loc")]
            public string? Loc { get; set; }

        }

        #endregion

        [TestMethod]
        public void TestKnownSitemap()
        {
            using var runner = TestRunner.Run(GetContent());

            var sitemap = GetSitemap(runner);

            AssertX.Contains("http://localhost/", sitemap);
            AssertX.Contains("http://localhost/other", sitemap);

            AssertX.Contains("http://localhost/children/", sitemap);
            AssertX.Contains("http://localhost/children/child-other", sitemap);

            Assert.AreEqual(4, sitemap.Count);
        }

        private static HashSet<string> GetSitemap(TestRunner runner)
        {
            var serializer = new XmlSerializer(typeof(UrlSet));

            using var response = runner.GetResponse("/" + Sitemap.FILE_NAME);

            var sitemap = serializer.Deserialize(response.GetResponseStream()) as UrlSet;

            return sitemap?.Entries?.Select(u => u.Loc!.Replace(":" + runner.Port, string.Empty)).ToHashSet() ?? new HashSet<string>();
        }

        private static IHandlerBuilder GetContent()
        {
            var root = Layout.Create();

            var children = Layout.Create();

            children.Index(Page.From("Child Index Page", "Child Index"));
            children.Add("child-other", Page.From("Child Other Page", "Child Other"));

            var content = Layout.Create();

            content.Index(Page.From("Index Page", "Index"));
            content.Add("other", Page.From("Other Page", "Other"));

            content.Add("children", children);

            root.Add(Sitemap.FILE_NAME, Sitemap.Create());

            root.Fallback(content);

            return root;
        }

    }

}
