using System.Net;
using Websocket.Client;
using WS = GenHTTP.Modules.Websockets.Websocket;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Legacy;

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

    [TestMethod]
    public async Task TestErrorHandling()
    {
        var server = WS.Create()
                       .OnOpen(_ => throw new InvalidOperationException("Ooops"));

        await using var host = await TestHost.RunAsync(server);

        using var client = new WebsocketClient(new Uri("ws://localhost:" + host.Port));

        await client.Start();

        await Task.Delay(1000);
    }

}
