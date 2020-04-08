using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

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

        [Fact]
        public void TestNotFounds()
        {
            using var runner = TestRunner.Run(GetContent());

            using var indexResponse = runner.GetResponse("/sitemaps/");
            Assert.Equal(HttpStatusCode.NotFound, indexResponse.StatusCode);

            using var fileResponse = runner.GetResponse("/sitemaps/some.txt");
            Assert.Equal(HttpStatusCode.NotFound, fileResponse.StatusCode);
        }

        private HashSet<string> GetSitemap(TestRunner runner)
        {
            var serializer = new XmlSerializer(typeof(UrlSet));

            using var response = runner.GetResponse("/sitemaps/sitemap.xml");

            var sitemap = (UrlSet)serializer.Deserialize(response.GetResponseStream());

            return sitemap.Entries.Select(u => u.Loc!.Replace(":" + runner.Port, string.Empty)).ToHashSet();
        }

        private IHandlerBuilder GetContent()
        {
            var root = Layout.Create();

            var children = Layout.Create();

            children.Index(Page.From("Child Index Page", "Child Index"));
            children.File("child-other", Page.From("Child Other Page", "Child Other"));

            var content = Layout.Create();

            content.Index(Page.From("Index Page", "Index"));
            content.File("other", Page.From("Other Page", "Other"));

            content.Section("children", children);

            root.Section("sitemaps", Sitemap.Create());

            root.Fallback(content);

            return root;
        }

    }

}
