using System.Net;
using GenHTTP.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WS = GenHTTP.Modules.Websockets.Websocket;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public sealed class ErrorHandlingTests
{

    [TestMethod]
    public async Task TestInvalidRequest()
    {
        using var host = TestHost.Run(WS.Create());

        using var response = await host.GetResponseAsync();
        
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
}
