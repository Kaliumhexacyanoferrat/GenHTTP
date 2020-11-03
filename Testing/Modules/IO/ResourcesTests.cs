using System.Net;

using Xunit;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class ResourcesTests
    {

        [Fact]
        public void TestFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/File.txt");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("This is text!", response.GetContent());
        }

        [Fact]
        public void TestSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/Subdirectory/AnotherFile.txt");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("This is another text!", response.GetContent());
        }

        [Fact]
        public void TestNoFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah.txt");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestNoSubdirectoryFileDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/File.txt");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestRootDownload()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly("Resources")));

            using var response = runner.GetResponse("/File.txt");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("This is text!", response.GetContent());
        }

        [Fact]
        public void TestDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestNonExistingDirectory()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = runner.GetResponse("/Resources/nah/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestContent()
        {
            var layout = Layout.Create()
                               .Add("sitemap", Sitemap.Create())
                               .Add("resources", Resources.From(ResourceTree.FromAssembly("Resources")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/sitemap");

            var sitemap = response.GetSitemap();

            Assert.Contains("/resources/Error.html", sitemap);
        }

    }

}
