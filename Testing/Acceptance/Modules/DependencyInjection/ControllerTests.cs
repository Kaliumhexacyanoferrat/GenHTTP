using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class ControllerTests
{

    #region Supporting Structures

    public class TestController(AwesomeService first)
    {

        public string DoWork(AnotherAwesomeService second) => $"{first.DoWork()}-{second.DoWork()}";

    }

    #endregion

    #region Functionality

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceDependencyInjection(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddDependentController<TestController>("t");

        await using var runner = await DependentHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync("/t/do-work/");

        Assert.AreEqual("42-24", await response.GetContentAsync());
    }

    #endregion

}
