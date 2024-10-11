using System.Net;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class RoutingTests
{

    /// <summary>
    ///     As a client, I expect the server to return 404 for non-existing files.
    /// </summary>
    [TestMethod]
    public async Task NotFoundForUnknownRoute()
    {
        using var runner = TestHost.Run(Layout.Create());

        using var response = await runner.GetResponseAsync();
        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }
}
