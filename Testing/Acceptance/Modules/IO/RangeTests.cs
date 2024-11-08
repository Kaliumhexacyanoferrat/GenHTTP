using System.Net;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class RangeTests
{
    private const string Content = "0123456789";

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangesAreOptional(TestEngine engine)
    {
        using var response = await GetResponse(engine, null);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual(Content, await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFullRangeIsSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-8");

        await response.AssertStatusAsync(HttpStatusCode.PartialContent);
        Assert.AreEqual("12345678", await response.GetContentAsync());
        Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangeFromStartIsSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=4-");

        await response.AssertStatusAsync(HttpStatusCode.PartialContent);
        Assert.AreEqual("456789", await response.GetContentAsync());
        Assert.AreEqual("bytes 4-9/10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangeFromEndIsSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=-4");

        await response.AssertStatusAsync(HttpStatusCode.PartialContent);
        Assert.AreEqual("6789", await response.GetContentAsync());
        Assert.AreEqual("bytes 6-9/10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSingleRangeIsSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-1");

        await response.AssertStatusAsync(HttpStatusCode.PartialContent);
        Assert.AreEqual("1", await response.GetContentAsync());
        Assert.AreEqual("bytes 1-1/10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFullRangeNotSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=9-13");

        await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangeFromStartNotSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=12-");

        await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangeFromEndNotSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=-12");

        await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultipleRangesNotSatisfied(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-2,3-4");

        await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOneBasedIndexDoesNotWork(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-10");

        await response.AssertStatusAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        Assert.AreEqual("bytes */10", response.GetContentHeader("Content-Range"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHeadRequest(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-8", HttpMethod.Head);

        await response.AssertStatusAsync(HttpStatusCode.PartialContent);

        Assert.AreEqual("bytes 1-8/10", response.GetContentHeader("Content-Range"));
        Assert.AreEqual("8", response.GetContentHeader("Content-Length"));

        Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangesIgnoredOnPostRequests(TestEngine engine)
    {
        using var response = await GetResponse(engine, "bytes=1-8", HttpMethod.Post);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual(Content, await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangesAreTaggedDifferently(TestEngine engine)
    {
        using var withRange = await GetResponse(engine, "bytes=1-8");
        using var withoutRange = await GetResponse(engine, null);

        Assert.AreNotEqual(withRange.GetHeader("ETag"), withoutRange.GetHeader("ETag"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAddSupportForSingleFile(TestEngine engine)
    {
        var download = Download.From(Resource.FromString("Hello World!"))
                               .AddRangeSupport();

        await using var runner = await TestHost.RunAsync(download);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("bytes", response.GetHeader("Accept-Ranges"));
    }

    private static async Task<HttpResponseMessage> GetResponse(TestEngine engine, string? requestedRange, HttpMethod? method = null)
    {
        await using var runner = await GetRunnerAsync(engine);

        var request = runner.GetRequest(method: method ?? HttpMethod.Get);

        if (requestedRange != null)
        {
            request.Headers.Add("Range", requestedRange);
        }

        return await runner.GetResponseAsync(request);
    }

    private static async Task<TestHost> GetRunnerAsync(TestEngine engine)
    {
        var content = GenHTTP.Modules.IO.Content.From(Resource.FromString(Content));

        content.AddRangeSupport();

        return await TestHost.RunAsync(content, engine: engine);
    }

}
