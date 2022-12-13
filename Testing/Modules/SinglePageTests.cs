using System.IO;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.IO;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class SinglePageTests
    {

        [TestMethod]
        public async Task TestIndex()
        {
            var root = CreateRoot();

            FileUtil.WriteText(Path.Combine(root, "index.html"), "This is the index!");

            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

            using var index = await runner.GetResponse("/");

            Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
            Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));

            var content = await index.GetContent();

            Assert.AreEqual("This is the index!", content);
        }

        [TestMethod]
        public async Task TestIndexServedWithRouting()
        {
            var root = CreateRoot();

            FileUtil.WriteText(Path.Combine(root, "index.html"), "This is the index!");

            var spa = SinglePageApplication.From(ResourceTree.FromDirectory(root))
                                           .ServerSideRouting();

            using var runner = TestRunner.Run(spa);

            using var index = await runner.GetResponse("/some-route/");

            Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
            Assert.AreEqual("text/html", index.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestNoIndex()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

            using var index = await runner.GetResponse("/");

            Assert.AreEqual(HttpStatusCode.NotFound, index.StatusCode);
        }

        [TestMethod]
        public async Task TestFile()
        {
            var root = CreateRoot();

            FileUtil.WriteText(Path.Combine(root, "some.txt"), "This is some text file :)");

            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(root)));

            using var index = await runner.GetResponse("/some.txt");

            Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
            Assert.AreEqual("text/plain", index.GetContentHeader("Content-Type"));

            var content = await index.GetContent();

            Assert.AreEqual("This is some text file :)", content);
        }

        [TestMethod]
        public async Task TestNoFile()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())));

            using var index = await runner.GetResponse("/nope.txt");

            Assert.AreEqual(HttpStatusCode.NotFound, index.StatusCode);
        }

        [TestMethod]
        public async Task TestContent()
        {
            var root = CreateRoot();

            FileUtil.WriteText(Path.Combine(root, "index.html"), "Index");
            FileUtil.WriteText(Path.Combine(root, "file.html"), "File");

            var layout = Layout.Create()
                               .Add("spa", SinglePageApplication.From(ResourceTree.FromDirectory(root)))
                               .Add("sitemap", Sitemap.Create());

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/sitemap");

            var sitemap = await response.GetSitemap();

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
