using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

using GenHTTP.Testing.Acceptance.Utilities;

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
    [MultiEngineFrameworkTest]
    public async Task TestInstance(TestEngine engine, ExecutionMode mode)
    {
        var controller = Controller.From(new TestController())
                                   .ExecutionMode(mode);

        var app = Layout.Create()
                        .Add(controller);

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
