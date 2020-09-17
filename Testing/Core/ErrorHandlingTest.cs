using System;
using System.Collections.Generic;
using System.Net;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ErrorHandlingTest
    {

        private class MalfunctioningRouter : IHandler
        {

            public IHandler Parent { get => throw new NotImplementedException(); }

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
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
