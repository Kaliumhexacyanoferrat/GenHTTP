using System.IO;

using Xunit;

using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Providers
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
                var layout = Layout.Create().File("file", Download.FromFile(file));

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
                File.Delete(file);
            }
        }

        /// <summary>
        /// As a developer, I can provide downloads from embedded files.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromResource()
        {
            var layout = Layout.Create().File("file", Download.FromResource("File.txt"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/file");

                Assert.Equal("This is text!", response.GetContent());
                Assert.Equal("text/plain", response.GetResponseHeader("Content-Type"));
            }
        }

    }

}
