using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class KeepAliveTests
{

    /// <summary>
    /// This is a white box test that verifies that two subsequent requests
    /// to the server from the same client reuse the same connection,
    /// thus verifying that keep-alive connections are supported.
    /// </summary>
    [TestMethod]
    public async Task TestKeepAlive()
    {
        Socket? connection = null;

        var tester = Inline.Create()
                           .Get((IRequest r) =>
                           {
                               var socket = r.Upgrade().Socket;

                               if (connection == null)
                               {
                                   connection = socket;
                               }
                               else
                               {
                                   if (connection != socket)
                                   {
                                       throw new ProviderException(ResponseStatus.BadRequest, "Client connection did change, so this is not a keep-alive request");
                                   }
                               }
                           });

        await using var runner = await TestHost.RunAsync(tester, engine: TestEngine.Internal);

        using var r1 = await runner.GetResponseAsync();
        await r1.AssertStatusAsync(HttpStatusCode.NoContent);

        using var r2 = await runner.GetResponseAsync();
        await r2.AssertStatusAsync(HttpStatusCode.NoContent);
    }

}
