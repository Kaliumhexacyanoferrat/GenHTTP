using System.IO;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public class SinglePageTests
    {

        [TestMethod]
        public void TestIndex()
        {
            var root = CreateRoot();

            File.WriteAllText(Path.Combine(root, "index.html"), "This is the index!");

            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

            using var index = runner.GetResponse("/");

            Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
            Assert.AreEqual("text/html", index.ContentType);

            var content = index.GetContent();

            Assert.AreEqual("This is the index!", content);
        }

        [TestMethod]
        public void TestNoIndex()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

            using var index = runner.GetResponse("/");

            Assert.AreEqual(HttpStatusCode.NotFound, index.StatusCode);
        }

        [TestMethod]
        public void TestFile()
        {
            var root = CreateRoot();

            File.WriteAllText(Path.Combine(root, "some.txt"), "This is some text file :)");

            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

            using var index = runner.GetResponse("/some.txt");

            Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
            Assert.AreEqual("text/plain", index.ContentType);

            var content = index.GetContent();

            Assert.AreEqual("This is some text file :)", content);
        }

        [TestMethod]
        public void TestNoFile()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

            using var index = runner.GetResponse("/nope.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, index.StatusCode);
        }

        [TestMethod]
        public void TestContent()
        {
            var root = CreateRoot();

            File.WriteAllText(Path.Combine(root, "index.html"), "Index");
            File.WriteAllText(Path.Combine(root, "file.html"), "File");

            var layout = Layout.Create()
                               .Add("spa", SinglePageApplication.From(ResourceTree.FromDirectory(root)))
                               .Add("sitemap", Sitemap.Create());

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/sitemap");

            var sitemap = response.GetSitemap();

            AssertX.Contains("/spa/index.html", sitemap);
            AssertX.Contains("/spa/file.html", sitemap);
        }

        private static string CreateRoot()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tempDirectory);

            return tempDirectory;
        }

    }

}
