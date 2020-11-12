using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public class ResultTypeTests
    {

        #region Supporting data structures

        public class TestController
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
        public void ControllerMayReturnHandlerBuilder()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/handler-builder/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("HandlerBuilder", response.GetContent());
        }

        [TestMethod]
        public void ControllerMayReturnHandler()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/handler/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Handler", response.GetContent());
        }

        [TestMethod]
        public void ControllerMayReturnResponseBuilder()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/response-builder/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("ResponseBuilder", response.GetContent());
        }

        [TestMethod]
        public void ControllerMayReturnResponse()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/response/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Response", response.GetContent());
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
