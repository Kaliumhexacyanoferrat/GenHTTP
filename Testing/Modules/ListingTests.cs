﻿using System.IO;
using System.Net;

using Xunit;

using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ListingTests
    {

        /// <summary>
        /// As an user of a web application, I can view the folders and files available
        /// on root level of a listed directory.
        /// </summary>
        [Fact]
        public void TestGetMainListing()
        {
            using var runner = GetEnvironment();

            using var response = runner.GetResponse("/");

            var content = response.GetContent();

            Assert.Contains("Subdirectory", content);
            Assert.Contains("With%20Spaces", content);

            Assert.Contains("my.txt", content);

            Assert.DoesNotContain("..", content);
        }

        /// <summary>
        /// As an user of a web application, I can view the folders and files available
        /// within a subdirectory of a listed directory.
        /// </summary>
        [Fact]
        public void TestGetSubdirectory()
        {
            using var runner = GetEnvironment();

            using var response = runner.GetResponse("/Subdirectory/");

            var content = response.GetContent();

            Assert.Contains("..", content);
        }


        /// <summary>
        /// As an user of a web application, I can download the files listed by the
        /// directory listing feature.
        /// </summary>
        [Fact]
        public void TestDownload()
        {
            using var runner = GetEnvironment();

            using var response = runner.GetResponse("/my.txt");

            Assert.Equal("Hello World!", response.GetContent());
        }

        [Fact]
        public void TestNonExistingFolder()
        {
            using var runner = GetEnvironment();

            using var response = runner.GetResponse("/idonotexist/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private TestRunner GetEnvironment()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Directory.CreateDirectory(tempFolder);

            Directory.CreateDirectory(Path.Combine(tempFolder, "Subdirectory"));

            Directory.CreateDirectory(Path.Combine(tempFolder, "With Spaces"));

            File.WriteAllText(Path.Combine(tempFolder, "my.txt"), "Hello World!");

            var listing = Listing.From(ResourceTree.FromDirectory(tempFolder));

            return TestRunner.Run(listing);
        }

    }

}
