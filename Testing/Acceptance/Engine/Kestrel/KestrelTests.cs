using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class KestrelTests
{

    [TestMethod]
    public async Task TestLifecycle()
    {
        var handler = Content.From(Resource.FromString("Hello Kestrel!")).Build();

        await using var host = new TestHost(handler, engine: TestEngine.Kestrel);

        await host.StartAsync();

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestHeaders()
    {
        var app = Inline.Create().Get((IRequest request) =>
        {
            var count = request.Headers.Count;

            Assert.AreEqual(count, request.Headers.Keys.Count());
            Assert.AreEqual(count, request.Headers.Values.Count());

            Assert.IsTrue(request.Headers.All(kv => kv.Value != string.Empty));

            Assert.IsTrue(request.Headers.ContainsKey("Host"));

            Assert.IsNotNull(request.Headers["Host"]);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: TestEngine.Kestrel);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestQuery()
    {
        var app = Inline.Create().Get((IRequest request) =>
        {
            var count = request.Query.Count;

            Assert.AreEqual(count, request.Query.Keys.Count());
            Assert.AreEqual(count, request.Query.Values.Count());

            Assert.IsTrue(request.Query.All(kv => kv.Value != string.Empty));

            Assert.IsTrue(request.Query.ContainsKey("a"));

            Assert.IsNotNull(request.Query["a"]);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: TestEngine.Kestrel);

        using var response = await host.GetResponseAsync("/?a=1&b=2");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestConnection()
    {
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


}
