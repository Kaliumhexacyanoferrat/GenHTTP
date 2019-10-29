using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ErrorHandlingTest
    {

        private class MalfunctioningRouter : IRouter
        {

            public IRouter Parent { get => throw new NotImplementedException(); set { } }

            public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
            {
                throw new NotImplementedException();
            }

            public IRenderer<TemplateModel> GetRenderer()
            {
                throw new NotImplementedException();
            }

            public void HandleContext(IEditableRoutingContext current)
            {
                throw new NotImplementedException();
            }

            public string? Route(string path, int currentDepth)
            {
                throw new NotImplementedException();
            }

        }

        [Fact]
        public void TestGenericError()
        {
            using var runner = TestRunner.Run(new MalfunctioningRouter());

            var request = runner.GetRequest();

            using var response = request.GetSafeResponse();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }

}
