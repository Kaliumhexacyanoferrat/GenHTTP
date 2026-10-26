using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public class TypeVisibilityTests
{

    private class MyPrivateService
    {

        [ResourceMethod]
        internal int Sum(int a, int b) => a + b;

    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestPrivateMembersCanBeAccessed(ServerEngine engine, ExecutionMode mode)
    {
        var api = ServiceResource.From<MyPrivateService>()
                                 .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync("/?a=1&b=5");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("6", await response.GetContentAsync());
    }

}
