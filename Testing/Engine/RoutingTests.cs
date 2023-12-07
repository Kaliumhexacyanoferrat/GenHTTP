using System.Net;
using System.Threading.Tasks;

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
        public async Task NotFoundForUnknownRoute()
        {
            using var runner = TestRunner.Run();

            using var response = await runner.GetResponse();
            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    }

}
