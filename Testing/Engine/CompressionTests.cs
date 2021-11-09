﻿using System.IO.Compression;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Basics;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class CompressionTests
    {

        private class CustomAlgorithm : ICompressionAlgorithm
        {

            public string Name => "custom";

            public Priority Priority => Priority.High;

            public IResponseContent Compress(IResponseContent content, CompressionLevel level)
            {
                return content;
            }

        }

        private class CustomAlgorithmBuilder : IBuilder<ICompressionAlgorithm>
        {

            public ICompressionAlgorithm Build()
            {
                return new CustomAlgorithm();
            }

        }

        /// <summary>
        /// As a developer, I expect responses to be compressed out of the box.
        /// </summary>
        [TestMethod]
        public void TestCompression()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, br");

            using var response = request.GetSafeResponse();

            Assert.AreEqual("br", response.ContentEncoding);
        }

        /// <summary>
        /// As a browser, I expect only supported compression algorithms to be used
        /// to generate my response.
        /// </summary>
        [TestMethod]
        public void TestSpecficAlgorithm()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = request.GetSafeResponse();

            Assert.AreEqual("gzip", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to disable compression.
        /// </summary>
        [TestMethod]
        public void TestCompressionDisabled()
        {
            using var runner = TestRunner.Run(false);

            using var response = runner.GetResponse();

            AssertX.IsNullOrEmpty(response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to add custom compression algorithms.
        /// </summary>
        [TestMethod]
        public void TestCustomCompression()
        {
            using var runner = new TestRunner();

            runner.Host.Compression(CompressedContent.Default().Add(new CustomAlgorithm()).Level(CompressionLevel.Optimal)).Start();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "custom");

            using var response = request.GetSafeResponse();

            Assert.AreEqual("custom", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want already compressed content not to be compressed again.
        /// </summary>
        [TestMethod]
        public void TestNoAdditionalCompression()
        {
            var image = Resource.FromString("Image!").Type(ContentType.ImageJpg);

            using var runner = TestRunner.Run(Layout.Create().Add("uncompressed", Content.From(image)));

            using var response = runner.GetResponse("/uncompressed");

            AssertX.IsNullOrEmpty(response.ContentEncoding);
        }

        [TestMethod]
        public void TestVariyHeaderAdded()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = request.GetSafeResponse();

            Assert.AreEqual("Accept-Encoding", response.GetResponseHeader("Vary"));
        }

        [TestMethod]
        public void TestVariyHeaderExtendedAdded()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                .Header("Vary", "Host")
                .Content(Resource.FromString("Hello World").Build())
                .Type(ContentType.TextHtml)
                .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap());

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = request.GetSafeResponse();

            Assert.AreEqual("Host, Accept-Encoding", response.GetResponseHeader("Vary"));
        }

    }

}
