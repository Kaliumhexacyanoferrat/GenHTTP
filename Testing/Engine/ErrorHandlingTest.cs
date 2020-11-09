using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine
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

            public ValueTask<IResponse?> HandleAsync(IRequest request)
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
