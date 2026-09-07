using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class MethodTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomMethods(ServerEngine engine)
    {
        var result = Inline.Create().On(() => "Hmm, Beer", [new("BREW")]);

        await using var host = await TestHost.RunAsync(result, engine: engine);

        var request = host.GetRequest(method: new HttpMethod("BREW"));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }
}
