using System.IO;
using System.Net;

using Xunit;

using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class SinglePageTests
    {

        [Fact]
        public void TestIndex()
        {
            var root = CreateRoot();

            File.WriteAllText(Path.Combine(root, "index.html"), "This is the index!");

            using var runner = TestRunner.Run(SinglePageApplication.From(root));

            using var index = runner.GetResponse("/");

            Assert.Equal(HttpStatusCode.OK, index.StatusCode);
            Assert.Equal("text/html", index.ContentType);

            var content = index.GetContent();

            Assert.Equal("This is the index!", content);
        }

        [Fact]
        public void TestNoIndex()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(CreateRoot()));

            using var index = runner.GetResponse("/");

            Assert.Equal(HttpStatusCode.NotFound, index.StatusCode);
        }

        [Fact]
        public void TestFile()
        {
            var root = CreateRoot();

            File.WriteAllText(Path.Combine(root, "some.txt"), "This is some text file :)");

            using var runner = TestRunner.Run(SinglePageApplication.From(root));

            using var index = runner.GetResponse("/some.txt");

            Assert.Equal(HttpStatusCode.OK, index.StatusCode);
            Assert.Equal("text/plain", index.ContentType);

            var content = index.GetContent();

            Assert.Equal("This is some text file :)", content);
        }

        [Fact]
        public void TestNoFile()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(CreateRoot()));

            using var index = runner.GetResponse("/nope.txt");

            Assert.Equal(HttpStatusCode.NotFound, index.StatusCode);
        }

        private static string CreateRoot()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tempDirectory);

            return tempDirectory;
        }

    }

}
