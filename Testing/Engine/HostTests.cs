using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class HostTests
    {

        [TestMethod]
        public void TestStart()
        {
            using var runner = new TestRunner();

            runner.Host.Start();

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestRestart()
        {
            using var runner = new TestRunner();

            runner.Host.Restart();

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
