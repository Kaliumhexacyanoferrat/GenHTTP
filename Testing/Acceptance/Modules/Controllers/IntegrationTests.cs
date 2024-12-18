using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public class IntegrationTests
{

    #region Supporting data structures

    public class TestController
    {

        [ControllerAction(RequestMethod.Get)]
        public string DoWork() => "Work done";

    }

    #endregion

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInstance(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Controller.From(new TestController()));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/do-work/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Work done", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestChaining()
    {
        Chain.Works(Controller.From<TestController>());
    }

}
