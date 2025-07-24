using GenHTTP.Modules.DependencyInjection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class InlineTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestParameterInjection(TestEngine engine)
    {
        var app = DependentInline.Create()
                                 .Get((AwesomeService s) => s.DoWork());

        await using var runner = await DependentHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

}
