using System;
using System.Collections.Generic;
using System.Net;

using Xunit;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;

using GenHTTP.Testing.Acceptance.Domain;

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
            using var runner = TestRunner.Run();

            using var response = runner.GetResponse();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
