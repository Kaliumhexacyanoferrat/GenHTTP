using System.Net;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Functional;

[TestClass]
public sealed class InlineTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetRoot(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => 42), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetPath(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get("/blubb", () => 42), engine: engine);

        using var response = await host.GetResponseAsync("/blubb");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetQueryParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((int param) => param + 1), engine: engine);

        using var response = await host.GetResponseAsync("/?param=41");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetEmptyBooleanQueryParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((bool param) => param), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("0", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetEmptyDoubleQueryParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((double param) => param), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("0", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetEmptyStringQueryParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((string param) => param), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetEmptyEnumQueryParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((EnumData param) => param), engine: engine);

        using var response = await host.GetResponseAsync("/?param=");

        Assert.AreEqual("One", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetPathParam(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(":param", (int param) => param + 1), engine: engine);

        using var response = await host.GetResponseAsync("/41");

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFound(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => 42), engine: engine);

        using var response = await host.GetResponseAsync("/nope");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRaw(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get((IRequest request) =>
        {
            return request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content("42");
        }), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStream(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => new MemoryStream("42"u8.ToArray())), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJson(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => new MyClass("42", 42, 42.0)), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostJson(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Post((MyClass input) => input), engine: engine);

        var request = host.GetRequest();

        request.Method = HttpMethod.Post;

        request.Content = new StringContent("{\"string\":\"42\",\"int\":42,\"double\":42}", Encoding.UTF8, "application/json");

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsync(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Get(async () =>
        {
            var stream = new MemoryStream();

            await stream.WriteAsync("42"u8.ToArray());

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHandlerBuilder(TestEngine engine)
    {
        var target = "https://www.google.de/";

        await using var host = await TestHost.RunAsync(Inline.Create().Get(() => Redirect.To(target)), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(target, response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHandler(TestEngine engine)
    {
        var target = "https://www.google.de/";

        await using var host = await TestHost.RunAsync(Inline.Create().Get((IHandler parent) => Redirect.To(target).Build()), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(target, response.GetHeader("Location"));
    }

    #region Supporting data structures

    public record MyClass(string String, int Int, double Double);

    private enum EnumData { One, Two }

    #endregion

}
