using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class WebserviceTests
{

    #region Supporting Structures

    public class TestService(AwesomeService first)
    {

        [ResourceMethod]
        public string DoWork(AnotherAwesomeService second) => $"{first.DoWork()}-{second.DoWork()}";

    }

    #endregion

    #region Functionality

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceDependencyInjection(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddDependentService<TestService>("service");

        await using var runner = await DependentHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync("/service");

        Assert.AreEqual("42-24", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestCustomization()
    {
        var app = Layout.Create()
                        .AddDependentService<TestService>("s", Injection.Default(), Serialization.Default(), Formatting.Default());

        await using var runner = await DependentHost.RunAsync(app);

        using var response = await runner.GetResponseAsync("/s");

        response.EnsureSuccessStatusCode();
    }

    #endregion

}
