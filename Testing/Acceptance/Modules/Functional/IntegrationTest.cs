using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Functional;

[TestClass]
public class IntegrationTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormatters(TestEngine engine)
    {
        var formatting = Formatting.Empty()
                                   .Add<BoolFormatter>();

        var api = Inline.Create()
                        .Any("get-bool", (bool value) => value)
                        .Formatters(formatting);

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync("/get-bool?value=1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("1", await response.GetContentAsync());
    }

}
