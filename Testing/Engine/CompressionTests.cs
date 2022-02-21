﻿using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public async Task TestCompression()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, br");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("br", response.Content.Headers.ContentEncoding.First());
        }

        /// <summary>
        /// As a browser, I expect only supported compression algorithms to be used
        /// to generate my response.
        /// </summary>
        [TestMethod]
        public async Task TestSpecficAlgorithm()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("gzip", response.Content.Headers.ContentEncoding.First());
        }

        /// <summary>
        /// As a developer, I want to be able to disable compression.
        /// </summary>
        [TestMethod]
        public async Task TestCompressionDisabled()
        {
            using var runner = TestRunner.Run(false);

            using var response = await runner.GetResponse();

            Assert.IsFalse(response.Content.Headers.ContentEncoding.Any());
        }

        /// <summary>
        /// As a developer, I want to be able to add custom compression algorithms.
        /// </summary>
        [TestMethod]
        public async Task TestCustomCompression()
        {
            using var runner = new TestRunner();

            runner.Host.Compression(CompressedContent.Default().Add(new CustomAlgorithm()).Level(CompressionLevel.Optimal)).Start();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "custom");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("custom", response.Content.Headers.ContentEncoding.First());
        }

        /// <summary>
        /// As a developer, I want already compressed content not to be compressed again.
        /// </summary>
        [TestMethod]
        public async Task TestNoAdditionalCompression()
        {
            var image = Resource.FromString("Image!").Type(ContentType.ImageJpg);

            using var runner = TestRunner.Run(Layout.Create().Add("uncompressed", Content.From(image)));

            using var response = await runner.GetResponse("/uncompressed");

            Assert.IsFalse(response.Content.Headers.ContentEncoding.Any());
        }

        [TestMethod]
        public async Task TestVariyHeaderAdded()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("Accept-Encoding", response.GetHeader("Vary"));
        }

        [TestMethod]
        public async Task TestVariyHeaderExtendedAdded()
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

            using var response = await runner.GetResponse(request);

            Assert.IsTrue(response.Headers.Vary.Contains("Host"));
            Assert.IsTrue(response.Headers.Vary.Contains("Accept-Encoding"));
        }

        [TestMethod]
        public async Task TestContentType()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Content(Resource.FromString("Hello World").Build())
                        .Type("application/json; charset=utf-8")
                        .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap());

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("br", response.Content.Headers.ContentEncoding.First());
        }

    }

}
