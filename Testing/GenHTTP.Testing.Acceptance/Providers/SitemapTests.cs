using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

using GenHTTP.Api.Routing;
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

        private IRouter GetContent()
        {
            var root = Layout.Create();

            var children = Layout.Create();

            children.Add("child-index", Page.From("Child Index Page", "Child Index"), true);
            children.Add("child-other", Page.From("Child Other Page", "Child Other"), false);

            var content = Layout.Create();

            content.Add("index", Page.From("Index Page", "Index"), true);
            content.Add("other", Page.From("Other Page", "Other"), false);

            content.Add("children", children);

            root.Add("sitemaps", Sitemap.From(content.Build()));

            root.Default(content);

            return root.Build();
        }

    }

}
