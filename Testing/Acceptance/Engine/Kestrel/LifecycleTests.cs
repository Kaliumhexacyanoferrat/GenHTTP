using System.Net;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class LifecycleTests
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

}
