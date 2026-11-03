using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class HeaderTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerHeaderCanBeSet(ServerEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Server", "TFB")
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("TFB", response.GetHeader("Server"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeHeadersAreWritten(ServerEngine engine)
    {
        var value = new string('x', 20_000);

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("X-Large", value)
                    .Header("X-Small", "small")
                    .Content("Hello World")
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual(value, response.GetHeader("X-Large"));
        Assert.AreEqual("small", response.GetHeader("X-Small"));
        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDateHeaderCanBeSet(ServerEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Header("Date", "123")
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("123", response.GetHeader("Date"));
    }

}
