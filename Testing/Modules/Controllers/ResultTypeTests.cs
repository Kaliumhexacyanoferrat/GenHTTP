using System.Net;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Mvc
{

    public class ResultTypeTests
    {

        #region Supporting data structures

        public class TestController
        {

            public IHandlerBuilder HandlerBuilder()
            {
                return Content.From("HandlerBuilder");
            }

            public IHandler Handler(IHandler parent)
            {
                return Content.From("Handler").Build(parent);
            }

            public IResponseBuilder ResponseBuilder(IRequest request)
            {
                return request.Respond()
                              .Content("ResponseBuilder");
            }

            public IResponse Response(IRequest request)
            {
                return request.Respond()
                              .Content("Response")
                              .Build();
            }

        }

        #endregion

        #region Tests

        [Fact]
        public void ControllerMayReturnHandlerBuilder()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/handler-builder/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("HandlerBuilder", response.GetContent());
        }

        [Fact]
        public void ControllerMayReturnHandler()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/handler/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Handler", response.GetContent());
        }

        [Fact]
        public void ControllerMayReturnResponseBuilder()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/response-builder/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("ResponseBuilder", response.GetContent());
        }

        [Fact]
        public void ControllerMayReturnResponse()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/response/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Response", response.GetContent());
        }

        #endregion

        #region Helpers

        private TestRunner GetRunner()
        {
            return TestRunner.Run(Layout.Create().AddController<TestController>("t"));
        }

        #endregion

    }

}
