using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Routing
{

    public class RoutingTests
    {

        /// <summary>
        /// As a client, I expect the server to return 404 for non-existing files.
        /// </summary>
        [Fact]
        public void NotFoundForUnknownRoute()
        {
            using (var runner = TestRunner.Run())
            {
                using var response = runner.GetResponse();
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

    }

}
