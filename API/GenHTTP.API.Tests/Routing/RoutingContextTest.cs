using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules;

namespace GenHTTP.Api.Tests.Routing
{

    public class RoutingContextTest
    {

        [Fact]
        public void TestScope()
        {
            var context = GetContext("/one/two");

            var childRouter = Mock.Of<IRouter>();

            context.Scope(childRouter, "one");

            Assert.Equal(childRouter, context.Router);
            Assert.Equal("/two", context.ScopedPath);
        }


        [Fact]
        public void TestScopeNoSegment()
        {
            var context = GetContext("/one/two");

            var childRouter = Mock.Of<IRouter>();
            context.Scope(childRouter);

            Assert.Equal(childRouter, context.Router);
            Assert.Equal("/one/two", context.ScopedPath);
        }

        [Fact]
        public void TestRegisterContent()
        {
            var context = GetContext();
            var content = Mock.Of<IContentProvider>();

            context.RegisterContent(content);

            Assert.Equal(content, context.ContentProvider);
        }

        [Fact]
        public void TestEmptyRouting()
        {
            var context = GetContext();

            Assert.Null(context.Route(string.Empty));
            Assert.Null(context.Route("  "));
        }

        [Fact]
        public void TestKnownRoute()
        {
            var context = GetContext();

            Assert.Equal("./test", context.Route("./test"));
            Assert.Equal("../test", context.Route("../test"));

            Assert.Null(context.Route("test"));
            Assert.Null(context.Route("test/one"));

            Assert.Equal("https://google.de", context.Route("https://google.de"));
            Assert.Equal("tel:123456", context.Route("tel:123456"));
        }

        [Fact]
        public void TestRouted()
        {
            TestRouting("/one/two", 1);
            TestRouting("/", 0);
        }

        private void TestRouting(string route, int expected)
        {
            var router = new Mock<IRouter>();

            router.Setup(r => r.Route(It.IsAny<string>(), It.Is<int>(depth => expected == depth)));

            GetContext(route, router);
        }

        private RoutingContext GetContext(string path = "/", IMock<IRouter>? router = null)
        {
            var coreRouter = router ?? new Mock<IRouter>();

            var request = new Mock<IHttpRequest>();
            request.SetupGet(r => r.Path).Returns(path);

            return new RoutingContext(coreRouter.Object, request.Object);
        }

    }

}
