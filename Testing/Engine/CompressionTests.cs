using System.IO.Compression;
using System.Threading.Tasks;

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
        public async Task TestCompression()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, br");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("br", response.GetHeader("Content-Encoding"));
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

            Assert.AreEqual("gzip", response.GetHeader("Content-Encoding"));
        }

        /// <summary>
        /// As a developer, I want to be able to disable compression.
        /// </summary>
        [TestMethod]
        public async Task TestCompressionDisabled()
        {
            using var runner = TestRunner.Run(false);

            using var response = await runner.GetResponse();

            AssertX.IsNullOrEmpty(response.GetHeader("Content-Encoding"));
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

            Assert.AreEqual("custom", response.GetHeader("Content-Encoding"));
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

            AssertX.IsNullOrEmpty(response.GetHeader("Content-Encoding"));
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

            Assert.AreEqual("Host, Accept-Encoding", response.GetHeader("Vary"));
        }

    }

}
