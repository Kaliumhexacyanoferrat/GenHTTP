using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Core.Routing;
using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Tests.Routing
{

    public class CoreRouterTest
    {

        [Fact]
        public void TestCannotRoute()
        {
            Assert.Null(Instance.Route("res/one/two", 2));
        }

        [Fact]
        public void TestHasNoParent()
        {
            Assert.Throws<NotSupportedException>(() => Instance.Parent);
        }

        [Fact]
        public void TestCannotSetParent()
        {
            Assert.Throws<NotSupportedException>(() => Instance.Parent = Mock.Of<IRouter>());
        }

        [Fact]
        public void TestHasRenderer()
        {
            var templateModel = new TemplateModel(Mock.Of<IHttpRequest>(), Mock.Of<IHttpResponse>(), "Title", "Content");
            var rendered = Instance.GetRenderer().Render(templateModel);

            Assert.Contains("Title", rendered);
            Assert.Contains("Content", rendered);
        }

        [Fact]
        public void TestDelegatesRouting()
        {
            var context = Mock.Of<IEditableRoutingContext>();

            var content = new Mock<IRouter>();

            new CoreRouter(content.Object).HandleContext(context);

            content.Verify(r => r.HandleContext(context), Times.Once);
        }

        [Fact]
        public void TestHandlesErrors()
        {
            var request = Mock.Of<IHttpRequest>();

            var response = new Mock<IHttpResponse>();

            var header = new Mock<IHttpResponseHeader>();
            header.SetupGet(h => h.Type).Returns(ResponseType.InternalServerError);

            response.SetupGet(r => r.Header).Returns(header.Object);

            var errorHandler = Instance.GetErrorHandler(request, response.Object);

            errorHandler.Handle(request, response.Object);
        }

        private IRouter Instance => new CoreRouter(Mock.Of<IRouter>());

    }

}
