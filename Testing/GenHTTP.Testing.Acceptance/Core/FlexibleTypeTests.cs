using System.IO;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class FlexibleTypeTests
    {

        private class Provider : IContentProvider
        {

            public FlexibleContentType? ContentType => new FlexibleContentType("application/x-custom");

            public string? Title => null;

            public IResponseBuilder Handle(IRequest request)
            {
                return request.Respond()
                              .Content("Hello World!")
                              .Type("application/x-custom")
                              .Status(256, "Custom Status");
            }

        }

        /// <summary>
        /// As a developer I would like to use status codes and content types
        /// not supported by the server.
        /// </summary>
        [Fact]
        public void TestFlexibleStatus()
        {
            var content = Layout.Create().Add("index", new Provider(), true);

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.Equal(256, (int)response.StatusCode);
            Assert.Equal("Custom Status", response.StatusDescription);

            Assert.Equal("application/x-custom", response.ContentType);
        }

    }

}
