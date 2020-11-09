using System.Net;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    public class SeoTests
    {

        #region Supporting data structures

        public class TestController
        {

            public IHandlerBuilder Action()
            {
                return Content.From(Resource.FromString("Action"));
            }

            [ControllerAction(RequestMethod.DELETE)]
            public IHandlerBuilder Action([FromPath] int id)
            {
                return Content.From(Resource.FromString(id.ToString()));
            }

        }

        #endregion

        #region Tests

        /// <summary>
        /// As the developer of a web application, I don't want the MCV framework to generate duplicate content
        /// by accepting upper case letters in action names.
        /// </summary>
        [Fact]
        public void TestActionCasingMatters()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/Action/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
