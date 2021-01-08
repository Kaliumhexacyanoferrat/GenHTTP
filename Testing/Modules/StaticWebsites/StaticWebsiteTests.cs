using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.StaticWebsites;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.StaticWebsites
{

    [TestClass]
    public sealed class StaticWebsiteTests
    {

        [TestMethod]
        public void TestWithIndex()
        {
            var tree = VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index 1"))
                                  .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")));

            using var runner = TestRunner.Run(StaticWebsite.From(tree));

            using var indexResponse = runner.GetResponse();
            Assert.AreEqual("Index 1", indexResponse.GetContent());

            using var subIndexResponse = runner.GetResponse("/sub/");
            Assert.AreEqual("Index 2", subIndexResponse.GetContent());
        }

        [TestMethod]
        public void TestNoIndex()
        {
            var tree = VirtualTree.Create()
                                  .Add("sub", VirtualTree.Create());

            using var runner = TestRunner.Run(StaticWebsite.From(tree));

            using var indexResponse = runner.GetResponse();
            Assert.AreEqual(HttpStatusCode.NotFound, indexResponse.StatusCode);

            using var subIndexResponse = runner.GetResponse("/sub/");
            Assert.AreEqual(HttpStatusCode.NotFound, subIndexResponse.StatusCode);
        }

        [TestMethod]
        public void TestSitemap()
        {
            var tree = VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index 1"))
                                  .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")))
                                  .Add("sub2", VirtualTree.Create());

            using var runner = TestRunner.Run(StaticWebsite.From(tree));

            using var response = runner.GetResponse("/" + Sitemap.FILE_NAME);

            var sitemap = response.GetSitemap();

            Assert.AreEqual(2, sitemap.Count);
        }

        [TestMethod]
        public void TestNoSitemap()
        {
            using var runner = TestRunner.Run(StaticWebsite.From(VirtualTree.Create()).Sitemap(null));

            using var response = runner.GetResponse("/" + Sitemap.FILE_NAME);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestSitemapOverride()
        {
            var tree = VirtualTree.Create()
                                  .Add(Sitemap.FILE_NAME, Resource.FromString("Custom Sitemap")); 

            using var runner = TestRunner.Run(StaticWebsite.From(tree));

            using var response = runner.GetResponse("/" + Sitemap.FILE_NAME);
            Assert.AreEqual("Custom Sitemap", response.GetContent());
        }

        [TestMethod]
        public void TestRobots()
        {
            using var runner = TestRunner.Run(StaticWebsite.From(VirtualTree.Create()));

            using var response = runner.GetResponse("/" + BotInstructions.FILE_NAME);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestNoRobots()
        {
            using var runner = TestRunner.Run(StaticWebsite.From(VirtualTree.Create()).Robots(null));

            using var response = runner.GetResponse("/" + BotInstructions.FILE_NAME);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestRobotsOverride()
        {
            var tree = VirtualTree.Create()
                                  .Add(BotInstructions.FILE_NAME, Resource.FromString("Custom Robots"));

            using var runner = TestRunner.Run(StaticWebsite.From(tree));

            using var response = runner.GetResponse("/" + BotInstructions.FILE_NAME);
            Assert.AreEqual("Custom Robots", response.GetContent());
        }

        [TestMethod]
        public void TestNoRobotsInSubdirectory()
        {
            using var runner = TestRunner.Run(StaticWebsite.From(VirtualTree.Create()));

            using var response = runner.GetResponse("/sub/" + BotInstructions.FILE_NAME);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
