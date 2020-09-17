using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Sitemaps;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class SitemapTests
    {

        #region Helping data structures

        [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
        public class UrlSet
        {

            [XmlElement("url")]
            public List<Url>? Entries { get; set; }

        }

        public class Url
        {

            [XmlElement("loc")]
            public string? Loc { get; set; }

        }

        #endregion

        [Fact]
        public void TestKnownSitemap()
        {
            using var runner = TestRunner.Run(GetContent());

            var sitemap = GetSitemap(runner);

            Assert.Contains("http://localhost/", sitemap);
            Assert.Contains("http://localhost/other", sitemap);

            Assert.Contains("http://localhost/children/", sitemap);
            Assert.Contains("http://localhost/children/child-other", sitemap);

            Assert.Equal(4, sitemap.Count);
        }

        private HashSet<string> GetSitemap(TestRunner runner)
        {
            var serializer = new XmlSerializer(typeof(UrlSet));

            using var response = runner.GetResponse("/sitemap.xml");

            var sitemap = (UrlSet)serializer.Deserialize(response.GetResponseStream());

            return sitemap.Entries.Select(u => u.Loc!.Replace(":" + runner.Port, string.Empty)).ToHashSet();
        }

        private IHandlerBuilder GetContent()
        {
            var root = Layout.Create();

            var children = Layout.Create();

            children.Index(Page.From("Child Index Page", "Child Index"));
            children.Add("child-other", Page.From("Child Other Page", "Child Other"));

            var content = Layout.Create();

            content.Index(Page.From("Index Page", "Index"));
            content.Add("other", Page.From("Other Page", "Other"));

            content.Add("children", children);

            root.Add("sitemap.xml", Sitemap.Create());

            root.Fallback(content);

            return root;
        }

    }

}
