using System.Net;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class DownloadTests
    {

        [TestMethod]
        public void TestDownload()
        {
            using var runner = TestRunner.Run(Download.From(Resource.FromAssembly("File.txt")));

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("This is text!", response.GetContent());
            Assert.AreEqual("text/plain", response.GetResponseHeader("Content-Type"));
        }

        [TestMethod]
        public void TestDownloadDoesNotAcceptRouting()
        {
            var layout = Layout.Create()
                               .Add("file.txt", Download.From(Resource.FromAssembly("File.txt")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/file.txt/blubb");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void DownloadsCannotBeModified()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"));

            using var runner = TestRunner.Run(download);

            var request = runner.GetRequest();

            request.Method = "PUT";
            request.ContentType = "text/plain";

            using (var input = request.GetRequestStream())
            {
                input.Write(Encoding.UTF8.GetBytes("Hello World!"));
            }

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void TestFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"))
                                   .FileName("myfile.txt");

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetResponseHeader("Content-Disposition"));
        }

        [TestMethod]
        public void TestNoFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"));

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.AreEqual("attachment", response.GetResponseHeader("Content-Disposition"));
        }
        
        [TestMethod]
        public void TestFileNameFromResource()
        {
            var download = Download.From(Resource.FromAssembly("File.txt").Name("myfile.txt"));

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.AreEqual("attachment; filename=\"myfile.txt\"", response.GetResponseHeader("Content-Disposition"));
        }

    }

}
