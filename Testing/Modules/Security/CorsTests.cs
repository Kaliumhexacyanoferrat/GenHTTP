using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;

namespace GenHTTP.Testing.Acceptance.Modules.Security
{

    [TestClass]
    public sealed class CorsTests
    {

        [TestMethod]
        public async Task TestPreflight()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            var request = new HttpRequestMessage(HttpMethod.Options, runner.GetUrl("/t"));

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task TestPermissive()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            using var response = await runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Origin"));

            Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Methods"));
            Assert.AreEqual("*, Authorization", response.GetHeader("Access-Control-Allow-Headers"));
            Assert.AreEqual("*", response.GetHeader("Access-Control-Expose-Headers"));

            Assert.AreEqual("true", response.GetHeader("Access-Control-Allow-Credentials"));

            Assert.AreEqual("86400", response.GetHeader("Access-Control-Max-Age"));

            Assert.AreEqual("Hello World", await response.GetContent());
        }

        [TestMethod]
        public async Task TestPermissiveWithoutDefaultAuthorizationHeader()
        {
            using var runner = GetRunner(CorsPolicy.Permissive(false));

            using var response = await runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Origin"));

            Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Methods"));
            Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Headers"));
            Assert.AreEqual("*", response.GetHeader("Access-Control-Expose-Headers"));

            Assert.AreEqual("true", response.GetHeader("Access-Control-Allow-Credentials"));

            Assert.AreEqual("86400", response.GetHeader("Access-Control-Max-Age"));

            Assert.AreEqual("Hello World", await response.GetContent());
        }

        [TestMethod]
        public async Task TestRestrictive()
        {
            using var runner = GetRunner(CorsPolicy.Restrictive());

            using var response = await runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));

            Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Methods"));
            Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Headers"));
            Assert.IsFalse(response.Headers.Contains("Access-Control-Expose-Headers"));

            Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Credentials"));

            Assert.IsFalse(response.Headers.Contains("Access-Control-Max-Age"));
        }

        [TestMethod]
        public async Task TestCustom()
        {
            var policy = CorsPolicy.Restrictive()
                                   .Add("http://google.de", new List<FlexibleRequestMethod>() { new FlexibleRequestMethod(RequestMethod.GET) }, null, new List<string>() { "Accept" }, false);

            using var runner = GetRunner(policy);

            var request = runner.GetRequest("/t");
            request.Headers.Add("Origin", "http://google.de");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("http://google.de", response.GetHeader("Access-Control-Allow-Origin"));

            Assert.AreEqual("GET", response.GetHeader("Access-Control-Allow-Methods"));

            Assert.AreEqual("Accept", response.GetHeader("Access-Control-Expose-Headers"));

            Assert.AreEqual("Origin", response.GetHeader("Vary"));
        }

        private static TestRunner GetRunner(CorsPolicyBuilder policy)
        {
            var handler = Layout.Create()
                                .Add("t", Content.From(Resource.FromString("Hello World")))
                                .Add(policy);

            return TestRunner.Run(handler);
        }

    }

}
