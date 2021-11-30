using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public class RangeTests
    {
        private const string CONTENT = "0123456789";

        [TestMethod]
        public async Task TestRangesAreOptional()
        {
            using var response = await GetResponse(null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(CONTENT, await response.GetContent());
        }

        [TestMethod]
        public async Task TestFullRangeIsSatisfied()
        {
            using var response = await GetResponse("bytes=1-8");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("12345678", await response.GetContent());
            Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromStartIsSatisfied()
        {
            using var response = await GetResponse("bytes=4-");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("456789", await response.GetContent());
            Assert.AreEqual("bytes 4-9/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromEndIsSatisfied()
        {
            using var response = await GetResponse("bytes=-4");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("6789", await response.GetContent());
            Assert.AreEqual("bytes 6-9/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestSingleRangeIsSatisfied()
        {
            using var response = await GetResponse("bytes=1-1");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("1", await response.GetContent());
            Assert.AreEqual("bytes 1-1/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestFullRangeNotSatisfied()
        {
            using var response = await GetResponse("bytes=9-13");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromStartNotSatisfied()
        {
            using var response = await GetResponse("bytes=12-");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromEndNotSatisfied()
        {
            using var response = await GetResponse("bytes=-12");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestMultipleRangesNotSatisfied()
        {
            using var response = await GetResponse("bytes=1-2,3-4");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestOneBasedIndexDoesNotWork()
        {
            using var response = await GetResponse("bytes=1-10");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestHeadRequest()
        {
            using var response = await GetResponse("bytes=1-8", HttpMethod.Head);

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);

            Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
            Assert.AreEqual("8", response.GetContentHeader("Content-Length"));

            Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
        }

        [TestMethod]
        public async Task TestRangesIgnoredOnPostRequests()
        {
            using var response = await GetResponse("bytes=1-8", HttpMethod.Post);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(CONTENT, await response.GetContent());
        }

        [TestMethod]
        public async Task TestRangesAreTaggedDifferently()
        {
            using var withRange = await GetResponse("bytes=1-8");
            using var withoutRange = await GetResponse(null);

            Assert.AreNotEqual(withRange.GetHeader("ETag"), withoutRange.GetHeader("ETag"));
        }

        [TestMethod]
        public async Task TestAddSupportForSingleFile()
        {
            var download = Download.From(Resource.FromString("Hello World!"))
                                   .AddRangeSupport();

            using var runner = TestRunner.Run(download);

            using var response = await runner.GetResponse();

            Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
        }

        [TestMethod]
        public async Task TestUnknownLengthCannotBeRanged()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"))
                                   .AddRangeSupport();

            using var runner = TestRunner.Run(download);

            var request = runner.GetRequest();
            request.Headers.Add("Range", "bytes=1-2");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            AssertX.IsNullOrEmpty(response.GetHeader("Accept-Ranges"));
            AssertX.IsNullOrEmpty(response.GetContentHeader("Content-Length"));
        }

        private static async Task<HttpResponseMessage> GetResponse(string? requestedRange, HttpMethod? method = null)
        {
            using var runner = GetRunner();

            var request = runner.GetRequest(method: method ?? HttpMethod.Get);

            if (requestedRange != null)
            {
                request.Headers.Add("Range", requestedRange);
            }

            return await runner.GetResponse(request);
        }

        private static TestRunner GetRunner()
        {
            var content = Content.From(Resource.FromString(CONTENT));

            return TestRunner.Run(content, rangeSupport: true);
        }

    }

}
