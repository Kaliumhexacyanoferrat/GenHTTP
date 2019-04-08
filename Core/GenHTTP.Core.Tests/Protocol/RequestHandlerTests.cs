using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Core.Protocol;
using GenHTTP.Core.Infrastructure;

namespace GenHTTP.Core.Tests.Protocol
{

    public class RequestHandlerTests
    {

        [Fact]
        public void TestCoreError()
        {
            var server = new Mock<IServer>();

            server.Setup(s => s.Extensions).Returns(new ExtensionCollection());

            var request = new Mock<IRequest>();

            request.Setup(r => r.Respond()).Returns(new ResponseBuilder(request.Object));
            
            var handler = new RequestHandler(server.Object);

            var response = handler.Handle(request.Object, out var error);

            Assert.Equal(ResponseStatus.InternalServerError, response.Status.KnownStatus);
        }

    }

}
