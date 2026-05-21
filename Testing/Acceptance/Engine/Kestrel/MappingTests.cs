using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class MappingTests
{

    [TestMethod]
    public async Task TestHeaders()
    {
        if (!Engines.KestrelEnabled()) return;

        var app = Inline.Create().Get((IRequest request) =>
        {
            var headers = request.Header.Headers;

            Assert.IsTrue(headers.ContainsKey("Host"));
            
            var host = headers.GetEntry("Host");

            Assert.IsNotNull(host);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: TestEngine.Kestrel);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestQuery()
    {
        if (!Engines.KestrelEnabled()) return;

        var app = Inline.Create().Get((IRequest request) =>
        {
            var query = request.Header.Query;
            
            Assert.IsTrue(query.ContainsKey("a"));
            
            var a = query.GetEntry("a");

            Assert.IsNotNull(a);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: TestEngine.Kestrel);

        using var response = await host.GetResponseAsync("/?a=1&b=2");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    // todo
    
    /*
    [TestMethod]
    public async Task TestConnection()
    {
        if (!Engines.KestrelEnabled()) return;

        var app = Inline.Create().Get((IRequest request) =>
        {
            Assert.IsNotNull(request.Client.Host);

            Assert.AreEqual(ClientProtocol.Http, request.Client.Protocol);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: TestEngine.Kestrel);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }
    */

}
