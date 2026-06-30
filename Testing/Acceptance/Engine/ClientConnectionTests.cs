using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ClientConnectionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientIsExposed(TestEngine engine)
    {
        // the client connection is a pooled instance that gets reset once the request has
        // been handled, so its state must be captured immediately while handling the request
        IPAddress? address = null;
        ClientProtocol? protocol = null;
        X509Certificate? certificate = null;

        var handler = Inline.Create().Get((IRequest r) =>
        {
            address = r.Client.Address;
            protocol = r.Client.Protocol;
            certificate = r.Client.Certificate;

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler, engine: engine);

        using var _ = await runner.GetResponseAsync();

        Assert.IsTrue(IPAddress.IsLoopback(address!));
        Assert.AreEqual(ClientProtocol.Http, protocol);
        Assert.IsNull(certificate);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientIsResetBetweenRequests(TestEngine engine)
    {
        // the client connection is a pooled instance reused across requests on the same
        // connection, so its state must be captured immediately while handling each request
        var addresses = new List<IPAddress?>();

        var handler = Inline.Create().Get((IRequest r) =>
        {
            addresses.Add(r.Client.Address);

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler, engine: engine);

        using var first = await runner.GetResponseAsync();
        using var second = await runner.GetResponseAsync();

        Assert.AreEqual(2, addresses.Count);

        Assert.IsTrue(IPAddress.IsLoopback(addresses[0]!));
        Assert.IsTrue(IPAddress.IsLoopback(addresses[1]!));
    }

}
