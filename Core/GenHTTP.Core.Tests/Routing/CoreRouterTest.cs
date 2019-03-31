using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Core.Routing;
using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Core.Protocol;

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
            var templateModel = new TemplateModel(Mock.Of<IRequest>(), "Title", "Content");
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
            var request = new Mock<IRequest>();

            request.Setup(r => r.Respond()).Returns(new ResponseBuilder(request.Object));            

            var errorHandler = Instance.GetErrorHandler(request.Object, ResponseType.NotFound);

            var response = errorHandler.Handle(request.Object).Type(ResponseType.NotFound).Build();

            Assert.Equal(ResponseType.NotFound, response.Type);
        }

        private IRouter Instance => new CoreRouter(Mock.Of<IRouter>());

    }

}
