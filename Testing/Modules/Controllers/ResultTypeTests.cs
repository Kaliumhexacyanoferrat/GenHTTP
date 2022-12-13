using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class ResultTypeTests
    {

        #region Supporting data structures

        public sealed class TestController
        {

            public IHandlerBuilder HandlerBuilder()
            {
                return Content.From(Resource.FromString("HandlerBuilder"));
            }

            public IHandler Handler(IHandler parent)
            {
                return Content.From(Resource.FromString("Handler")).Build(parent);
            }

            public IResponseBuilder ResponseBuilder(IRequest request)
            {
                return request.Respond().Content("ResponseBuilder");
            }

            public IResponse Response(IRequest request)
            {
                return request.Respond().Content("Response").Build();
            }

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task ControllerMayReturnHandlerBuilder()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/handler-builder/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("HandlerBuilder", await response.GetContent());
        }

        [TestMethod]
        public async Task ControllerMayReturnHandler()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/handler/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Handler", await response.GetContent());
        }

        [TestMethod]
        public async Task ControllerMayReturnResponseBuilder()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/response-builder/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("ResponseBuilder", await response.GetContent());
        }

        [TestMethod]
        public async Task ControllerMayReturnResponse()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/response/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Response", await response.GetContent());
        }

        #endregion

        #region Helpers

        private static TestRunner GetRunner()
        {
            var controller = Controller.From<TestController>()
                                       .Formats(Serialization.Default())
                                       .Injectors(Injection.Default());

            return TestRunner.Run(Layout.Create().Add("t", controller));
        }

        #endregion

    }

}
