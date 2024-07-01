using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ResourcesTests
    {

        [TestMethod]
        public async Task TestFileDownload()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/File.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("This is text!", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestSubdirectoryFileDownload()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/Subdirectory/AnotherFile.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("This is another text!", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestNoFileDownload()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/nah.txt");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestNoSubdirectoryFileDownload()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/nah/File.txt");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestRootDownload()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly("Resources")));

            using var response = await runner.GetResponseAsync("/File.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("This is text!", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestDirectory()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/nah/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestNonExistingDirectory()
        {
            using var runner = TestHost.Run(Resources.From(ResourceTree.FromAssembly()));

            using var response = await runner.GetResponseAsync("/Resources/nah/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    }

}
