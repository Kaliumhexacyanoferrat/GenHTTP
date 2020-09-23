using System.Net;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class RoutingTests
    {

        /// <summary>
        /// As a client, I expect the server to return 404 for non-existing files.
        /// </summary>
        [Fact]
        public void NotFoundForUnknownRoute()
        {
            using var runner = TestRunner.Run();

            using var response = runner.GetResponse();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
