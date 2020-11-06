using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class FlexibleTypeTests
    {

        private class Provider : IHandler
        {

            public IHandler Parent => throw new System.NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new System.NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return request.Respond()
                              .Content("Hello World!")
                              .Type("application/x-custom")
                              .Status(256, "Custom Status")
                              .BuildTask();
            }

        }

        /// <summary>
        /// As a developer I would like to use status codes and content types
        /// not supported by the server.
        /// </summary>
        [Fact]
        public void TestFlexibleStatus()
        {
            var content = Layout.Create().Index(new Provider().Wrap());

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.Equal(256, (int)response.StatusCode);
            Assert.Equal("Custom Status", response.StatusDescription);

            Assert.Equal("application/x-custom", response.ContentType);
        }

    }

}
