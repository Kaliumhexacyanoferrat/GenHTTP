using System.Net;
using System.Threading.Tasks;

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
        public async Task TestWithIndex()
        {
            var tree = VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index 1"))
                                  .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")));

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var indexResponse = await runner.GetResponseAsync();
            Assert.AreEqual("Index 1", await indexResponse.GetContent());

            using var subIndexResponse = await runner.GetResponseAsync("/sub/");
            Assert.AreEqual("Index 2", await subIndexResponse.GetContent());
        }

        [TestMethod]
        public async Task TestNoIndex()
        {
            var tree = VirtualTree.Create()
                                  .Add("sub", VirtualTree.Create());

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var indexResponse = await runner.GetResponseAsync();
            await indexResponse.AssertStatusAsync(HttpStatusCode.NotFound);

            using var subIndexResponse = await runner.GetResponseAsync("/sub/"); 
            await subIndexResponse.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestSitemap()
        {
            var tree = VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index 1"))
                                  .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")))
                                  .Add("sub2", VirtualTree.Create());

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var response = await runner.GetResponseAsync("/" + Sitemap.FILE_NAME);

            var sitemap = await response.GetSitemap();

            Assert.AreEqual(2, sitemap.Count);
        }

        [TestMethod]
        public async Task TestNoSitemap()
        {
            using var runner = TestHost.Run(StaticWebsite.From(VirtualTree.Create()).Sitemap(null));

            using var response = await runner.GetResponseAsync("/" + Sitemap.FILE_NAME);

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestSitemapOverride()
        {
            var tree = VirtualTree.Create()
                                  .Add(Sitemap.FILE_NAME, Resource.FromString("Custom Sitemap")); 

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var response = await runner.GetResponseAsync("/" + Sitemap.FILE_NAME);
            Assert.AreEqual("Custom Sitemap", await response.GetContent());
        }

        [TestMethod]
        public async Task TestRobots()
        {
            using var runner = TestHost.Run(StaticWebsite.From(VirtualTree.Create()));

            using var response = await runner.GetResponseAsync("/" + BotInstructions.FILE_NAME);
            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task TestNoRobots()
        {
            using var runner = TestHost.Run(StaticWebsite.From(VirtualTree.Create()).Robots(null));

            using var response = await runner.GetResponseAsync("/" + BotInstructions.FILE_NAME);

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestRobotsOverride()
        {
            var tree = VirtualTree.Create()
                                  .Add(BotInstructions.FILE_NAME, Resource.FromString("Custom Robots"));

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var response = await runner.GetResponseAsync("/" + BotInstructions.FILE_NAME);
            Assert.AreEqual("Custom Robots", await response.GetContent());
        }

        [TestMethod]
        public async Task TestNoRobotsInSubdirectory()
        {
            using var runner = TestHost.Run(StaticWebsite.From(VirtualTree.Create()));

            using var response = await runner.GetResponseAsync("/sub/" + BotInstructions.FILE_NAME);

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    }

}
