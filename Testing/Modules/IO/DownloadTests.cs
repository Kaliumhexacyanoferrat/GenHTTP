using System.IO;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class DownloadTests
    {

        /// <summary>
        /// As a developer, I can provide downloads from file-based resources.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromFile()
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(file, "Hello File!");

            try
            {
                var layout = Layout.Create().Add("file", Download.FromFile(file));

                using (var runner = TestRunner.Run(layout))
                {
                    using (var response = runner.GetResponse("/file"))
                    {
                        Assert.Equal("Hello File!", response.GetContent());
                    }
                }
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        /// <summary>
        /// As a developer, I can provide downloads from embedded files.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromResource()
        {
            var layout = Layout.Create().Add("file", Download.FromResource("File.txt"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/file");

                Assert.Equal("This is text!", response.GetContent());
                Assert.Equal("text/plain", response.GetResponseHeader("Content-Type"));
            }
        }

        [Fact]
        public void DownloadsCannotBeModified()
        {
            var download = Download.FromResource("File.txt");

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
            var download = Download.FromString("Some File")
                                   .FileName("myfile.txt");

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.Equal("attachment; filename=\"myfile.txt\"", response.GetResponseHeader("Content-Disposition"));
        }

        [Fact]
        public void TestNoFileName()
        {
            var download = Download.FromString("Some File");

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.Equal("attachment", response.GetResponseHeader("Content-Disposition"));
        }

    }

}
