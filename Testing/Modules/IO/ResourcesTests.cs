using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public class ResourcesTests
    {

        [TestMethod]
        public void TestFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/File.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is text!", response.GetContent());
        }

        [TestMethod]
        public void TestSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/Subdirectory/AnotherFile.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is another text!", response.GetContent());
        }

        [TestMethod]
        public void TestNoFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestNoSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/File.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestRootDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly("Resources")));

            using var response = runner.GetResponse("/File.txt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("This is text!", response.GetContent());
        }

        [TestMethod]
        public void TestDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestNonExistingDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestContent()
        {
            var layout = Layout.Create()
                               .Add("sitemap", Sitemap.Create())
                               .Add("resources", Resources.From(ResourceTree.FromAssembly("Resources")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/sitemap");

            var sitemap = response.GetSitemap();

            AssertX.Contains("/resources/Error.html", sitemap);
        }

    }

}
