using System;
using System.Net;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.Templating;
using System.Collections.Generic;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ErrorHandlingTest
    {

        private class MalfunctioningRouter : IRouter
        {

            public IRouter Parent { get => throw new NotImplementedException(); set { } }

            public IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
            {
                throw new NotImplementedException();
            }

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
