using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.VirtualHosting;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules
{

    [TestClass]
    public sealed class VirtualHostsTests
    {

        /// <summary>
        /// As a hoster, I would like to provide several domains using the
        /// same server instance.
        /// </summary>
        [TestMethod]
        public async Task TestDomains()
        {
            var hosts = VirtualHosts.Create()
                                    .Add("domain1.com", Content.From(Resource.FromString("domain1.com")))
                                    .Add("domain2.com", Content.From(Resource.FromString("domain2.com")))
                                    .Default(Layout.Create().Index(Content.From(Resource.FromString("default"))));

            using var runner = TestRunner.Run(hosts);

            await TestHost(runner, "domain1.com");
            await TestHost(runner, "domain2.com");

            await TestHost(runner, "localhost", "default");
        }

        /// <summary>
        /// As a developer, I expect the server to return no content if
        /// no given route matches.
        /// </summary>
        [TestMethod]
        public async Task TestNoDefault()
        {
            using var runner = TestRunner.Run(VirtualHosts.Create());

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static async Task TestHost(TestRunner runner, string host, string? expected = null)
        {
            var request = runner.GetRequest();
            request.Headers.Add("Host", host);

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(expected ?? host, await response.GetContent());
        }

    }

}
