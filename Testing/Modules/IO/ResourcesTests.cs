using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ResourcesTests
    {

        [TestMethod]
        public async Task TestFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/File.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is text!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/Subdirectory/AnotherFile.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is another text!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestNoFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/nah.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestNoSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/nah/File.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestRootDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly("Resources")));

            using var response = await runner.GetResponse("/File.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is text!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/nah/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestNonExistingDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponse("/Resources/nah/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestContent()
        {
            var layout = Layout.Create()
                               .Add("sitemap", Sitemap.Create())
                               .Add("resources", Resources.From(ResourceTree.FromAssembly("Resources")));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/sitemap");

            var sitemap = await response.GetSitemap();

            AssertX.Contains("/resources/Error.html", sitemap);
        }

    }

}
