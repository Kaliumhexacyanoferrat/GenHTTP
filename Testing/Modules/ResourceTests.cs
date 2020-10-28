﻿using System.IO;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ResourceTests
    {

        /// <summary>
        /// As a developer, I can provide resources as static files.
        /// </summary>
        [Fact]
        public void TestGetDownloadFromResource()
        {
            var resources = Resources.FromAssembly("Resources");

            using var runner = TestRunner.Run(resources);

            using var response = runner.GetResponse("/File.txt");

            Assert.Equal("This is text!", response.GetContent());
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
                var layout = Layout.Create().Add("res", Resources.FromDirectory(file.Directory.FullName));

                using (var runner = TestRunner.Run(layout))
                {
                    using (var response = runner.GetResponse($"/res/{file.Name}"))
                    {
                        Assert.Equal("Hello File!", response.GetContent());
                    }
                }
            }
            finally
            {
                try { file.Delete(); } catch { /* nop */ }
            }
        }

    }

}