using System.Net;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class RoutingTests
{

    /// <summary>
    /// As a client, I expect the server to return 404 for non-existing files.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task NotFoundForUnknownRoute(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: engine);

        using var response = await runner.GetResponseAsync();
        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

}
