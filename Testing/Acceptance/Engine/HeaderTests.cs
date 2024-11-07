using System.Net;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class HeaderTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerHeaderCanBeSet(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Server", "TFB")
                    .Build();
        });

        using var runner = TestHost.Run(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("TFB", response.GetHeader("Server"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestReservedHeaderCannotBeSet(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Date", "123")
                    .Build();
        });

        using var runner = TestHost.Run(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

}
