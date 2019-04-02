using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class StaticTests
    {

        /// <summary>
        /// As a developer, I can provide resources as static files.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromResource()
        {
            var layout = Layout.Create().Add("res", Static.Resources("Resources"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/res/File.txt");

                Assert.Equal("This is text!", response.GetContent());
            }
        }

        /// <summary>
        /// As a developer, I can provide resources from file system.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromFileSystem()
        {
            var file = new FileInfo(Path.GetTempFileName());
            File.WriteAllText(file.FullName, "Hello File!");

            try
            {
                var layout = Layout.Create().Add("res", Static.Files(file.Directory.FullName));

                using (var runner = TestRunner.Run(layout))
                {
                    using var response = runner.GetResponse($"/res/{file.Name}");

                    Assert.Equal("Hello File!", response.GetContent());
                }
            }
            finally
            {
                file.Delete();
            }
        }

    }

}
