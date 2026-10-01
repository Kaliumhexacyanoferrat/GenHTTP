using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ParameterTests
{

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestCanReadSimpleTypesFromBody(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Post(([FromBody] string body) => body)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await PostAsync(runner, "1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("1", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestConversionError(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Post(([FromBody] int number) => number)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await PostAsync(runner, "ABC");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestQueryStringIsDecoded(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Get((string q) => $"[{q}]")
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        await AssertContentAsync(runner, "/?q=a%20b", "[a b]");
        await AssertContentAsync(runner, "/?q=a+b", "[a b]");
        await AssertContentAsync(runner, "/?q=%C3%A4", "[ä]");
        await AssertContentAsync(runner, "/?q=100%25", "[100%]");
        await AssertContentAsync(runner, "/?q=plain", "[plain]");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestQueryNumberIsDecoded(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Get((int n) => n)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        await AssertContentAsync(runner, "/?n=%2B42", "42");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestPathStringIsDecoded(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Get("/:name/", (string name) => $"[{name}]")
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        await AssertContentAsync(runner, "/a%20b/", "[a b]");
        await AssertContentAsync(runner, "/a+b/", "[a+b]");
        await AssertContentAsync(runner, "/%C3%A4/", "[ä]");
        await AssertContentAsync(runner, "/plain/", "[plain]");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestBodyStringIsNotDecoded(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Post(([FromBody] string q) => $"[{q}]")
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent("a%20b+ä", null, "text/plain");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("[a%20b+ä]", await response.GetContentAsync());
    }

    private static async Task AssertContentAsync(TestHost host, string path, string expected)
    {
        using var response = await host.GetResponseAsync(path);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual(expected, await response.GetContentAsync());
    }

    private static Task<HttpResponseMessage> PostAsync(TestHost host, string body)
    {
        var request = host.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent(body, null, "text/plain");

        return host.GetResponseAsync(request);
    }

    #endregion

}
