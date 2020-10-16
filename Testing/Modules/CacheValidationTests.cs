using System.Net;

using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Modules
{

    public class CacheValidationTests
    {

        [Fact]
        public void TestETagIsGenerated()
        {
            using var runner = TestRunner.Run(Content.From("Hello World!"));

            using var response = runner.GetResponse();

            var eTag = response.GetResponseHeader("ETag");

            Assert.NotNull(eTag);

            Assert.StartsWith("\"", eTag);
            Assert.EndsWith("\"", eTag);
        }

        [Fact]
        public void TestServerReturnsUnmodified()
        {
            using var runner = TestRunner.Run(Content.From("Hello World!"));

            using var response = runner.GetResponse();

            var eTag = response.GetResponseHeader("ETag");

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", eTag);

            using var cached = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);

            Assert.Equal("0", cached.GetResponseHeader("Content-Length"));
        }

        [Fact]
        public void TestServerReturnsModified()
        {
            using var runner = TestRunner.Run(Content.From("Hello World!"));

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", "\"123\"");

            using var reloaded = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.OK, reloaded.StatusCode);
        }

        [Fact]
        public void TestNoContentNoEtag()
        {
            var noContent = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond().Status(Api.Protocol.ResponseStatus.NoContent).Build();
            });

            using var runner = TestRunner.Run(noContent.Wrap());

            using var response = runner.GetResponse();

            Assert.DoesNotContain("ETag", response.Headers.AllKeys);
        }

        [Fact]
        public void TestOtherMethodNoETag()
        {
            using var runner = TestRunner.Run(Content.From("Hello World!"));

            var request = runner.GetRequest();

            request.Method = "DELETE";

            using var response = runner.GetResponse(request);

            Assert.DoesNotContain("ETag", response.Headers.AllKeys);
        }

    }

}
