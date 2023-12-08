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

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual(CONTENT, await response.GetContent());
        }

        [TestMethod]
        public async Task TestFullRangeIsSatisfied()
        {
            using var response = await GetResponse("bytes=1-8");

            await response.AssertStatusAsync(HttpStatusCode.PartialContent);
            Assert.AreEqual("12345678", await response.GetContent());
            Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromStartIsSatisfied()
        {
            using var response = await GetResponse("bytes=4-");

            await response.AssertStatusAsync(HttpStatusCode.PartialContent);
            Assert.AreEqual("456789", await response.GetContent());
            Assert.AreEqual("bytes 4-9/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromEndIsSatisfied()
        {
            using var response = await GetResponse("bytes=-4");

            await response.AssertStatusAsync(HttpStatusCode.PartialContent);
            Assert.AreEqual("6789", await response.GetContent());
            Assert.AreEqual("bytes 6-9/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestSingleRangeIsSatisfied()
        {
            using var response = await GetResponse("bytes=1-1");

            await response.AssertStatusAsync(HttpStatusCode.PartialContent);
            Assert.AreEqual("1", await response.GetContent());
            Assert.AreEqual("bytes 1-1/10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestFullRangeNotSatisfied()
        {
            using var response = await GetResponse("bytes=9-13");

            await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromStartNotSatisfied()
        {
            using var response = await GetResponse("bytes=12-");

            await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestRangeFromEndNotSatisfied()
        {
            using var response = await GetResponse("bytes=-12");

            await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestMultipleRangesNotSatisfied()
        {
            using var response = await GetResponse("bytes=1-2,3-4");

            await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestOneBasedIndexDoesNotWork()
        {
            using var response = await GetResponse("bytes=1-10");

            await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
            Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
        }

        [TestMethod]
        public async Task TestHeadRequest()
        {
            using var response = await GetResponse("bytes=1-8", HttpMethod.Head);

            await response.AssertStatusAsync(HttpStatusCode.PartialContent);

            Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
            Assert.AreEqual("8", response.GetContentHeader("Content-Length"));

            Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
        }

        [TestMethod]
        public async Task TestRangesIgnoredOnPostRequests()
        {
            using var response = await GetResponse("bytes=1-8", HttpMethod.Post);

            await response.AssertStatusAsync(HttpStatusCode.OK);
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

            using var runner = TestHost.Run(download);

            using var response = await runner.GetResponseAsync();

            Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
        }

        private static async Task<HttpResponseMessage> GetResponse(string? requestedRange, HttpMethod? method = null)
        {
            using var runner = GetRunner();

            var request = runner.GetRequest(method: method ?? HttpMethod.Get);

            if (requestedRange != null)
            {
                request.Headers.Add("Range", requestedRange);
            }

            return await runner.GetResponseAsync(request);
        }

        private static TestHost GetRunner()
        {
            var content = Content.From(Resource.FromString(CONTENT));

            content.AddRangeSupport();

            return TestHost.Run(content);
        }

    }

}
