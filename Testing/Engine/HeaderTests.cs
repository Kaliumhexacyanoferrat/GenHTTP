using System.Net;

using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class HeaderTests
    {

        [TestMethod]
        public void TestServerHeaderCanBeSet()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Header("Server", "TFB")
                        .Build();
            });

            using var runner = TestRunner.Run(handler);

            using var response = runner.GetResponse();

            Assert.AreEqual("TFB", response.Server);
        }

        [TestMethod]
        public void TestReservedHeaderCannotBeSet()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Header("Date", "123")
                        .Build();
            });

            using var runner = TestRunner.Run(handler);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }

}
