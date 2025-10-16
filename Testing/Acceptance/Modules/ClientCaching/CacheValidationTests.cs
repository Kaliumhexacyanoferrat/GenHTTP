using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.ClientCaching;

[TestClass]
public sealed class CacheValidationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestETagIsGenerated(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        using var response = await runner.GetResponseAsync();

        var eTag = response.GetHeader("ETag");

        Assert.IsNotNull(eTag);

        AssertX.StartsWith("\"", eTag);
        AssertX.EndsWith("\"", eTag);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReturnsUnmodified(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        using var response = await runner.GetResponseAsync();

        var eTag = response.GetHeader("ETag");

        var request = runner.GetRequest();

        request.Headers.Add("If-None-Match", eTag);

        using var cached = await runner.GetResponseAsync(request);

        await cached.AssertStatusAsync(HttpStatusCode.NotModified);

        if (engine == TestEngine.Internal)
        {
            Assert.AreEqual("0", cached.GetContentHeader("Content-Length"));
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerReturnsModified(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        var request = runner.GetRequest();

        request.Headers.Add("If-None-Match", "\"123\"");

        using var reloaded = await runner.GetResponseAsync(request);

        await reloaded.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoContentNoEtag(TestEngine engine)
    {
        var noContent = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond().Status(ResponseStatus.NoContent).Build();
        });

        await using var runner = await TestHost.RunAsync(noContent, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.IsFalse(response.Headers.Contains("ETag"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOtherMethodNoETag(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Content.From(Resource.FromString("Hello World!")), engine: engine);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Delete;

        using var response = await runner.GetResponseAsync(request);

        Assert.IsFalse(response.Headers.Contains("ETag"));
    }

}
