using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WS = GenHTTP.Modules.Websockets.Websocket;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public sealed class ErrorHandlingTests
{

    [TestMethod]
    public async Task TestInvalidRequest()
    {
        await using var host = await TestHost.RunAsync(WS.Create());

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

}
