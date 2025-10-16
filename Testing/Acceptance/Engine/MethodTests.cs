using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class MethodTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomMethods(TestEngine engine)
    {
        var result = Inline.Create().On(() => "Hmm, Beer", [FlexibleRequestMethod.Get("BREW")]);

        await using var host = await TestHost.RunAsync(result, engine: engine);

        var request = host.GetRequest(method: new HttpMethod("BREW"));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }
}
