using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class WebResourceTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLifecycle(TestEngine engine)
    {
        var port = TestHost.NextPort();

        var resource = Resource.FromWeb($"http://localhost:{port}/source");

        var app = Layout.Create()
                        .Add("source", Content.From(Resource.FromString("Hello World")))
                        .Add("target", Content.From(resource));

        var runner = new TestHost(app.Build(), engine: engine);

        runner.Host.Port((ushort)port).Handler(app);

        await runner.StartAsync();

        using var client = new HttpClient();

        using var response = await client.GetAsync($"http://localhost:{port}/target");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

}
