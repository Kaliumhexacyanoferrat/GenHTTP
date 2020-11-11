using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.VirtualHosting;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules
{

    [TestClass]
    public class VirtualHostsTests
    {

        /// <summary>
        /// As a hoster, I would like to provide several domains using the
        /// same server instance.
        /// </summary>
        [TestMethod]
        public void TestDomains()
        {
            var hosts = VirtualHosts.Create()
                                    .Add("domain1.com", Layout.Create().Fallback(Content.From(Resource.FromString("domain1.com"))))
                                    .Add("domain2.com", Layout.Create().Fallback(Content.From(Resource.FromString("domain2.com"))))
                                    .Default(Layout.Create().Index(Content.From(Resource.FromString("default"))));

            using var runner = TestRunner.Run(hosts);

            TestHost(runner, "domain1.com");
            TestHost(runner, "domain2.com");

            TestHost(runner, "localhost", "default");
        }

        /// <summary>
        /// As a developer, I expect the server to return no content if
        /// no given route matches.
        /// </summary>
        [TestMethod]
        public void TestNoDefault()
        {
            using var runner = TestRunner.Run(VirtualHosts.Create());

            using var response = runner.GetRequest().GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private void TestHost(TestRunner runner, string host, string? expected = null)
        {
            var request = runner.GetRequest();
            request.Headers.Add("Host", host);

            using var response = request.GetSafeResponse();

            Assert.AreEqual(expected ?? host, response.GetContent());
        }

    }

}
