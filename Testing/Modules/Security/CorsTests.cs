using System.Collections.Generic;
using System.Net;

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
        public void TestPreflight()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            var request = runner.GetRequest("/t");

            request.Method = "OPTIONS";

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void TestPermissive()
        {
            using var runner = GetRunner(CorsPolicy.Permissive());

            using var response = runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("*", response.GetResponseHeader("Access-Control-Allow-Origin"));

            Assert.AreEqual("*", response.GetResponseHeader("Access-Control-Allow-Methods"));
            Assert.AreEqual("*", response.GetResponseHeader("Access-Control-Allow-Headers"));
            Assert.AreEqual("*", response.GetResponseHeader("Access-Control-Expose-Headers"));

            Assert.AreEqual("true", response.GetResponseHeader("Access-Control-Allow-Credentials"));

            Assert.AreEqual("86400", response.GetResponseHeader("Access-Control-Max-Age"));

            Assert.AreEqual("Hello World", response.GetContent());
        }

        [TestMethod]
        public void TestRestrictive()
        {
            using var runner = GetRunner(CorsPolicy.Restrictive());

            using var response = runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            AssertX.DoesNotContain("Access-Control-Allow-Origin", response.Headers.AllKeys);

            AssertX.DoesNotContain("Access-Control-Allow-Methods", response.Headers.AllKeys);
            AssertX.DoesNotContain("Access-Control-Allow-Headers", response.Headers.AllKeys);
            AssertX.DoesNotContain("Access-Control-Expose-Headers", response.Headers.AllKeys);

            AssertX.DoesNotContain("Access-Control-Allow-Credentials", response.Headers.AllKeys);

            AssertX.DoesNotContain("Access-Control-Max-Age", response.Headers.AllKeys);
        }

        [TestMethod]
        public void TestCustom()
        {
            var policy = CorsPolicy.Restrictive()
                                   .Add("http://google.de", new List<FlexibleRequestMethod>() { new FlexibleRequestMethod(RequestMethod.GET) }, null, new List<string>() { "Accept" }, false);

            using var runner = GetRunner(policy);

            var request = runner.GetRequest("/t");

            request.Headers.Add("Origin", "http://google.de");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("http://google.de", response.GetResponseHeader("Access-Control-Allow-Origin"));

            Assert.AreEqual("GET", response.GetResponseHeader("Access-Control-Allow-Methods"));

            Assert.AreEqual("Accept", response.GetResponseHeader("Access-Control-Expose-Headers"));

            Assert.AreEqual("Origin", response.GetResponseHeader("Vary"));
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
