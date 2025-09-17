using System.Net;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

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

}
