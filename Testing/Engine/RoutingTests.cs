using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class RoutingTests
    {

        /// <summary>
        /// As a client, I expect the server to return 404 for non-existing files.
        /// </summary>
        [TestMethod]
        public void NotFoundForUnknownRoute()
        {
            using var runner = TestRunner.Run();

            using var response = runner.GetResponse();
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
