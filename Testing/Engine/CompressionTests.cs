using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class CompressionTests
    {

        private class CustomAlgorithm : ICompressionAlgorithm
        {

            public string Name => "custom";

            public Priority Priority => Priority.High;

            public IResponseContent Compress(IResponseContent content)
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
        [Fact]
        public void TestCompression()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, br");

            using var response = request.GetSafeResponse();

            Assert.Equal("br", response.ContentEncoding);
        }

        /// <summary>
        /// As a browser, I expect only supported compression algorithms to be used
        /// to generate my response.
        /// </summary>
        [Fact]
        public void TestSpecficAlgorithm()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip");

            using var response = request.GetSafeResponse();

            Assert.Equal("gzip", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to disable compression.
        /// </summary>
        [Fact]
        public void TestCompressionDisabled()
        {
            using var runner = TestRunner.Run(false);

            using var response = runner.GetResponse();

            Assert.Null(response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to add custom compression algorithms.
        /// </summary>
        [Fact]
        public void TestCustomCompression()
        {
            using var runner = new TestRunner();

            runner.Host.Compression(CompressedContent.Default().Add(new CustomAlgorithm())).Start();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "custom");

            using var response = request.GetSafeResponse();

            Assert.Equal("custom", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want already compressed content not to be compressed again.
        /// </summary>
        [Fact]
        public void TestNoAdditionalCompression()
        {
            var image = Resource.FromString("Image!").Type(new FlexibleContentType(ContentType.ImageJpg));

            using var runner = TestRunner.Run(Layout.Create().Add("uncompressed", Content.From(image)));

            using var response = runner.GetResponse("/uncompressed");

            Assert.Null(response.ContentEncoding);
        }

    }

}
