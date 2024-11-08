using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class SeoTests
{

    #region Tests

    /// <summary>
    /// As the developer of a web application, I don't want the MCV framework to generate duplicate content
    /// by accepting upper case letters in action names.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestActionCasingMatters(TestEngine engine)
    {
        await using var runner = await GetRunnerAsync(engine);

        using var response = await runner.GetResponseAsync("/t/Action/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helpers

    private async Task<TestHost> GetRunnerAsync(TestEngine engine) => await TestHost.RunAsync(Layout.Create().AddController<TestController>("t"), engine: engine);

    #endregion

    #region Supporting data structures

    public sealed class TestController
    {

        public IHandlerBuilder Action() => Content.From(Resource.FromString("Action"));

        [ControllerAction(RequestMethod.Delete)]
        public IHandlerBuilder Action([FromPath] int id) => Content.From(Resource.FromString(id.ToString()));
    }

    #endregion

}
