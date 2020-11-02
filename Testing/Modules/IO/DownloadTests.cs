using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class DownloadTests
    {

        [Fact]
        public void TestDownload()
        {
            using var runner = TestRunner.Run(Download.From(Resource.FromAssembly("File.txt")));

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal("This is text!", response.GetContent());
            Assert.Equal("text/plain", response.GetResponseHeader("Content-Type"));
        }

        [Fact]
        public void TestDownloadDoesNotAcceptRouting()
        {
            var layout = Layout.Create()
                               .Add("file.txt", Download.From(Resource.FromAssembly("File.txt")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/file.txt/blubb");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
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

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public void TestFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"))
                                   .FileName("myfile.txt");

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.Equal("attachment; filename=\"myfile.txt\"", response.GetResponseHeader("Content-Disposition"));
        }

        [Fact]
        public void TestNoFileName()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"));

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.Equal("attachment", response.GetResponseHeader("Content-Disposition"));
        }
        
        [Fact]
        public void TestFileNameFromResource()
        {
            var download = Download.From(Resource.FromAssembly("File.txt").Name("myfile.txt"));

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.Equal("attachment; filename=\"myfile.txt\"", response.GetResponseHeader("Content-Disposition"));
        }

    }

}
