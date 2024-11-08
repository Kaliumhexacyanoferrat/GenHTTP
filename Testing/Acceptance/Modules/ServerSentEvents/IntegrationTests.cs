using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.ServerSentEvents;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestCustomFormatting()
    {
        var source = EventSource.Create()
                                .Formatting(Formatting.Empty())
                                .Generator(async (c) => await c.DataAsync(DateOnly.MaxValue));

        await using var host = await TestHost.RunAsync(source);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("data: \"9999-12-31\"\n\n", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestConcernChaining() => Chain.Works(EventSource.Create().Generator((_) => new()));

}
