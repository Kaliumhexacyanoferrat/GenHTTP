using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
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

        var source = Content.From(Resource.FromString("Hello World"));

        var target = Content.From(resource);

        using var response = await RunAsync(source, target, engine, port);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }
    
    [TestMethod]
    [MultiEngineTest]
    public async Task TestOverwrites(TestEngine engine)
    {
        var port = TestHost.NextPort();

        var resource = Resource.FromWeb(new Uri($"http://localhost:{port}/source"))
                               .Modified(new DateTime(2005, 12, 3, 12, 0, 0, DateTimeKind.Utc))
                               .Type(new(ContentType.ApplicationJson))
                               .Name("Test.txt");

        var source = Content.From(Resource.FromString("Hello World"));

        var target = Download.From(resource);

        using var response = await RunAsync(source, target, engine, port);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Sat, 03 Dec 2005 12:00:00 GMT", response.GetContentHeader("Last-Modified"));
        Assert.AreEqual("application/json", response.GetContentHeader("Content-Type"));
        Assert.AreEqual("attachment; filename=\"Test.txt\"", response.GetContentHeader("Content-Disposition"));
    }

    [TestMethod]
    public void TestBuilderDoesRequireHttp()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Resource.FromWeb("file:///test.txt").Build());
    }
    
    [TestMethod]
    public void TestBuilderDoesRequireAbsolute()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Resource.FromWeb("./test.txt").Build());
    }
    
    [TestMethod]
    public void TestBuilderDoesRequireUri()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Resource.FromWeb("http://").Build());
    }
    
    private async Task<HttpResponseMessage> RunAsync(IHandlerBuilder source, IHandlerBuilder target, TestEngine engine, int port)
    {
        var app = Layout.Create()
                        .Add("source", source)
                        .Add("target", target);

        var runner = new TestHost(app.Build(), engine: engine);

        runner.Host.Port((ushort)port).Handler(app);

        await runner.StartAsync();

        using var client = new HttpClient();

        return await client.GetAsync($"http://localhost:{port}/target");
    }
    
}
