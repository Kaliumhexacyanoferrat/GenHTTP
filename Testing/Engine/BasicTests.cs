using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class BasicTests
    {

        [TestMethod]
        public async Task TestBuilder()
        {
            using var runner = new TestRunner();

            runner.Host.RequestMemoryLimit(128)
                       .TransferBufferSize(128)
                       .RequestReadTimeout(TimeSpan.FromSeconds(2))
                       .Backlog(1);

            runner.Start();

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestLegacyHttp()
        {
            using var runner = TestRunner.Run();

            using var client = TestRunner.GetClient(version: new Version(1, 0));

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestConnectionClose()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Connection", "close");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsTrue(response.Headers.Connection.Contains("Close"));
        }

        [TestMethod]
        public async Task TestEmptyQuery()
        {
            using var runner = TestRunner.Run();

            using var response = await runner.GetResponse("/?");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task TestKeepalive()
        {
            using var runner = TestRunner.Run();

            using var response = await runner.GetResponse();

            Assert.IsTrue(response.Headers.Connection.Contains("Keep-Alive"));
        }

    }

}
