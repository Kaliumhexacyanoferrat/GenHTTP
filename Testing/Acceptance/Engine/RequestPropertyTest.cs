using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;
using GenHTTP.Modules.Functional;

using NSubstitute;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class RequestPropertyTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestProperties(TestEngine engine)
    {
        var app = Inline.Create().Get((IRequest request) =>
        {
            request.Properties["my"] = "value";

            Assert.IsTrue(request.Properties.TryGet<string>("my", out _));

            request.Properties.Clear("my");

            Assert.IsFalse(request.Properties.TryGet<string>("my", out _));

            Assert.ThrowsExactly<KeyNotFoundException>(() => request.Properties["my"]);

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetBodyTwiceThrows(TestEngine engine)
    {
        var app = Inline.Create().Get((IRequest request) =>
        {
            request.GetBody();

            Assert.ThrowsExactly<InvalidOperationException>(() => request.GetBody());

            return true;
        });

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHeaderAccessibleAfterBodyLoaded(TestEngine engine)
    {
        var app = Inline.Create().Get((IRequest request) =>
        {
            request.GetBody();

            return request.Header.Target.AsString(decode: false);
        });

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public void TestApplyWithoutEndpoint()
    {
        var request = new Request();

        var server = Substitute.For<IServer>();

        request.Apply(server);

        Assert.AreSame(server, request.Server);
        Assert.IsNotNull(request.Header);
        Assert.IsNotNull(request.Properties);
    }

}
