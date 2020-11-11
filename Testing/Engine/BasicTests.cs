using System;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class BasicTests
    {

        [TestMethod]
        public void TestBuilder()
        {
            using var runner = new TestRunner();

            runner.Host.RequestMemoryLimit(128)
                       .TransferBufferSize(128)
                       .RequestReadTimeout(TimeSpan.FromSeconds(2))
                       .Backlog(1);

            runner.Start();

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestLegacyHttp()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.ProtocolVersion = new Version(1, 0);

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestConnectionClose()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Connection", "close");

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestEmptyQuery()
        {
            using var response = TestRunner.Run().GetResponse("/?");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
