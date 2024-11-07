using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class MethodTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomMethods(TestEngine engine)
    {
        var result = Inline.Create().On(() => "Hmm, Beer", new()
        {
            FlexibleRequestMethod.Get("BREW")
        });

        using var host = TestHost.Run(result, engine: engine);

        var request = host.GetRequest(method: new HttpMethod("BREW"));

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }
}
