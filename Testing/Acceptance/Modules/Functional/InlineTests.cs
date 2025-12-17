using System.Net;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Redirects;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Functional;

[TestClass]
public sealed class InlineTests
{

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetRoot(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => 42).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetPath(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get("/blubb", () => 42).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/blubb");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetQueryParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((int param) => param + 1).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/?param=41");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetEmptyBooleanQueryParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((bool param) => param).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("0", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetEmptyDoubleQueryParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((double param) => param).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("0", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetEmptyStringQueryParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((string param) => param).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetEmptyEnumQueryParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((EnumData param) => param).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("One", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestGetPathParam(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(":param", (int param) => param + 1).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/41");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNotFound(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => 42).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync("/nope");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestRaw(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((IRequest request) =>
        {
            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content("42");
        }).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestStream(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => new MemoryStream("42"u8.ToArray())).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestJson(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => new MyClass("42", 42, 42.0)).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestPostJson(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Post((MyClass input) => input).ExecutionMode(mode), engine: engine);

        var request = host.GetRequest();

        request.Method = HttpMethod.Post;

        request.Content = new StringContent("{\"string\":\"42\",\"int\":42,\"double\":42}", Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestAsync(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(async () =>
        {
            var stream = new MemoryStream();

            await stream.WriteAsync("42"u8.ToArray());

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestHandlerBuilder(TestEngine engine, ExecutionMode mode)
    {
        var target = "https://www.google.de/";

        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => Redirect.To(target)).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(target, response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestHandler(TestEngine engine, ExecutionMode mode)
    {
        var target = "https://www.google.de/";

        await using var host = await TestHost.RunAsync(Inline.Create().Get((IHandler parent) => Redirect.To(target).Build()).ExecutionMode(mode), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(target, response.GetHeader("Location"));
    }

    #region Supporting data structures

    public record MyClass(string String, int Int, double Double);

    private enum EnumData { One, Two }

    #endregion

}
