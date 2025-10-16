using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.ServerSentEvents;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomFormatting(TestEngine engine)
    {
        var source = EventSource.Create()
                                .Formatting(Formatting.Empty())
                                .Generator(async c => await c.DataAsync(DateOnly.MaxValue));

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("data: \"9999-12-31\"\n\n", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestConcernChaining() => Chain.Works(EventSource.Create().Generator(_ => new()));

}
