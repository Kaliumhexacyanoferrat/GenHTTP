using System.Collections.Generic;
using System.Net;

using Xunit;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;

namespace GenHTTP.Testing.Acceptance.Modules.Security
{

    public class CorsTests
    {

        [Fact]
        public void TestPreflight()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            var request = runner.GetRequest("/t");

            request.Method = "OPTIONS";

            using var response = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void TestPermissive()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            using var response = runner.GetResponse("/t");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal("*", response.GetResponseHeader("Access-Control-Allow-Origin"));

            Assert.Equal("*", response.GetResponseHeader("Access-Control-Allow-Methods"));
            Assert.Equal("*", response.GetResponseHeader("Access-Control-Allow-Headers"));
            Assert.Equal("*", response.GetResponseHeader("Access-Control-Expose-Headers"));

            Assert.Equal("true", response.GetResponseHeader("Access-Control-Allow-Credentials"));

            Assert.Equal("86400", response.GetResponseHeader("Access-Control-Max-Age"));

            Assert.Equal("Hello World", response.GetContent());
        }

        [Fact]
        public void TestRestrictive()
        {
            using var runner = GetRunner(CorsPolicy.Restrictive());

            using var response = runner.GetResponse("/t");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.DoesNotContain("Access-Control-Allow-Origin", response.Headers.AllKeys);

            Assert.DoesNotContain("Access-Control-Allow-Methods", response.Headers.AllKeys);
            Assert.DoesNotContain("Access-Control-Allow-Headers", response.Headers.AllKeys);
            Assert.DoesNotContain("Access-Control-Expose-Headers", response.Headers.AllKeys);

            Assert.DoesNotContain("Access-Control-Allow-Credentials", response.Headers.AllKeys);

            Assert.DoesNotContain("Access-Control-Max-Age", response.Headers.AllKeys);
        }

        [Fact]
        public void TestCustom()
        {
            var policy = CorsPolicy.Restrictive()
                                   .Add("http://google.de", new List<FlexibleRequestMethod>() { new FlexibleRequestMethod(RequestMethod.GET) }, null, new List<string>() { "Accept" }, false);

            using var runner = GetRunner(policy);

            var request = runner.GetRequest("/t");

            request.Headers.Add("Origin", "http://google.de");

            using var response = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal("http://google.de", response.GetResponseHeader("Access-Control-Allow-Origin"));

            Assert.Equal("GET", response.GetResponseHeader("Access-Control-Allow-Methods"));

            Assert.Equal("Accept", response.GetResponseHeader("Access-Control-Expose-Headers"));

            Assert.Equal("Origin", response.GetResponseHeader("Vary"));
        }

        private TestRunner GetRunner(CorsPolicyBuilder policy)
        {
            var handler = Layout.Create()
                                .Add("t", Content.From(Resource.FromString("Hello World")))
                                .Add(policy);

            return TestRunner.Run(handler);
        }

    }

}
