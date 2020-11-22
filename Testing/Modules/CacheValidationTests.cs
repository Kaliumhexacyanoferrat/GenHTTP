using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules
{

    [TestClass]
    public sealed class CacheValidationTests
    {

        [TestMethod]
        public void TestETagIsGenerated()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse();

            var eTag = response.GetResponseHeader("ETag");

            Assert.IsNotNull(eTag);

            AssertX.StartsWith("\"", eTag);
            AssertX.EndsWith("\"", eTag);
        }

        [TestMethod]
        public void TestServerReturnsUnmodified()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse();

            var eTag = response.GetResponseHeader("ETag");

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", eTag);

            using var cached = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.NotModified, cached.StatusCode);

            Assert.AreEqual("0", cached.GetResponseHeader("Content-Length"));
        }

        [TestMethod]
        public void TestServerReturnsModified()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", "\"123\"");

            using var reloaded = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, reloaded.StatusCode);
        }

        [TestMethod]
        public void TestNoContentNoEtag()
        {
            var noContent = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond().Status(Api.Protocol.ResponseStatus.NoContent).Build();
            });

            using var runner = TestRunner.Run(noContent.Wrap());

            using var response = runner.GetResponse();

            AssertX.DoesNotContain("ETag", response.Headers.AllKeys);
        }

        [TestMethod]
        public void TestOtherMethodNoETag()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            var request = runner.GetRequest();

            request.Method = "DELETE";

            using var response = runner.GetResponse(request);

            AssertX.DoesNotContain("ETag", response.Headers.AllKeys);
        }

    }

}
