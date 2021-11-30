using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class DownloadTests
    {

        [TestMethod]
        public async Task TestDownload()
        {
            using var runner = TestRunner.Run(Download.From(Resource.FromAssembly("File.txt")));

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("This is text!", await response.GetContent());
            Assert.AreEqual("text/plain", response.GetContentHeader("Content-Type"));
        }

        [TestMethod]
        public async Task TestDownloadDoesNotAcceptRouting()
        {
            var layout = Layout.Create()
                               .Add("file.txt", Download.From(Resource.FromAssembly("File.txt")));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/file.txt/blubb");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DownloadsCannotBeModified()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"));

            using var runner = TestRunner.Run(download);

            var request = runner.GetRequest();

            request.Method = HttpMethod.Put;
            request.Content = new StringContent("Hello World!", Encoding.UTF8, "text/plain");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public async Task TestFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"))
                                   .FileName("myfile.txt");

            using var runner = TestRunner.Run(download);

            using var response = await runner.GetResponse();

            Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
        }

        [TestMethod]
        public async Task TestNoFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"));

            using var runner = TestRunner.Run(download);

            using var response = await runner.GetResponse();

            Assert.AreEqual("attachment", response.GetContentHeader("Content-Disposition"));
        }
        
        [TestMethod]
        public async Task TestFileNameFromResource()
        {
            var download = Download.From(Resource.FromAssembly("File.txt").Name("myfile.txt"));

            using var runner = TestRunner.Run(download);

            using var response = await runner.GetResponse();

            Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetContentHeader("Content-Disposition"));
        }

    }

}
