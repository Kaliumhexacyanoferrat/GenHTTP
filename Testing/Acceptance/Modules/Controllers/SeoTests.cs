using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class SeoTests
{

    #region Supporting data structures

    public sealed class TestController
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
    [TestMethod]
    public async Task TestActionCasingMatters()
    {
            using var runner = GetRunner();

            using var response = await runner.GetResponseAsync("/t/Action/");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    #endregion

    #region Helpers

    private TestHost GetRunner()
    {
            return TestHost.Run(Layout.Create().AddController<TestController>("t"));
        }

    #endregion

}
