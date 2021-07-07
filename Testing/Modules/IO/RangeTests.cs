using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public class RangeTests
    {
        private const string CONTENT = "0123456789";

        [TestMethod]
        public void TestRangesAreOptional()
        {
            using var response = GetResponse(null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(CONTENT, response.GetContent());
        }

        [TestMethod]
        public void TestFullRangeIsSatisfied()
        {
            using var response = GetResponse("bytes=1-8");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("12345678", response.GetContent());
            Assert.AreEqual("bytes 1-8/10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestRangeFromStartIsSatisfied()
        {
            using var response = GetResponse("bytes=4-");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("456789", response.GetContent());
            Assert.AreEqual("bytes 4-9/10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestRangeFromEndIsSatisfied()
        {
            using var response = GetResponse("bytes=-4");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("6789", response.GetContent());
            Assert.AreEqual("bytes 6-9/10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestSingleRangeIsSatisfied()
        {
            using var response = GetResponse("bytes=1-1");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual("1", response.GetContent());
            Assert.AreEqual("bytes 1-1/10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestFullRangeNotSatisfied()
        {
            using var response = GetResponse("bytes=9-13");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestRangeFromStartNotSatisfied()
        {
            using var response = GetResponse("bytes=12-");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestRangeFromEndNotSatisfied()
        {
            using var response = GetResponse("bytes=-12");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestMultipleRangesNotSatisfied()
        {
            using var response = GetResponse("bytes=1-2,3-4");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestRangeTypeNotSatisfied()
        {
            using var response = GetResponse("age=99");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestInvalidRangeNotSatisfied()
        {
            using var response = GetResponse("lorem ipsum");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestReverseRangeNotSatisfied()
        {
            using var response = GetResponse("bytes=8-1");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestOneBasedIndexDoesNotWork()
        {
            using var response = GetResponse("bytes=1-10");

            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.AreEqual("bytes */10", response.GetResponseHeader("Content-Range"));
        }

        [TestMethod]
        public void TestHeadRequest()
        {
            using var response = GetResponse("bytes=1-8", "HEAD");

            Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);

            Assert.AreEqual("bytes 1-8/10", response.GetResponseHeader("Content-Range"));
            Assert.AreEqual("8", response.GetResponseHeader("Content-Length"));

            Assert.AreEqual("bytes", response.GetResponseHeader("Accept-Ranges"));
        }

        [TestMethod]
        public void TestRangesIgnoredOnPostRequests()
        {
            using var response = GetResponse("bytes=1-8", "POST");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(CONTENT, response.GetContent());
        }

        [TestMethod]
        public void TestRangesAreTaggedDifferently()
        {
            using var withRange = GetResponse("bytes=1-8");
            using var withoutRange = GetResponse(null);

            Assert.AreNotEqual(withRange.GetResponseHeader("ETag"), withoutRange.GetResponseHeader("ETag"));
        }

        [TestMethod]
        public void TestAddSupportForSingleFile()
        {
            var download = Download.From(Resource.FromString("Hello World!"))
                                   .AddRangeSupport();

            using var runner = TestRunner.Run(download);

            using var response = runner.GetResponse();

            Assert.AreEqual("bytes", response.GetResponseHeader("Accept-Ranges"));
        }

        [TestMethod]
        public void TestUnknownLengthCannotBeRanged()
        {
            var download = Download.From(Resource.FromAssembly("File.txt"))
                                   .AddRangeSupport();

            using var runner = TestRunner.Run(download);


            var request = runner.GetRequest();

            request.Headers.Add("Range", "bytes=1-2");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual(string.Empty, response.GetResponseHeader("Accept-Ranges"));
            Assert.AreEqual(string.Empty, response.GetResponseHeader("Content-Length"));
        }

        private static HttpWebResponse GetResponse(string? requestedRange, string method = "GET")
        {
            using var runner = GetRunner();

            var request = runner.GetRequest();

            request.Method = method;

            if (requestedRange != null)
            {
                request.Headers.Add("Range", requestedRange);
            }

            return runner.GetResponse(request);
        }

        private static TestRunner GetRunner()
        {
            var content = Content.From(Resource.FromString(CONTENT));

            return TestRunner.Run(content, rangeSupport: true);
        }

    }

}
