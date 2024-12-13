using System.Net;
using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.OpenApi;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ApiBrowsing;

[TestClass]
public class HandlerTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetOnly(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(GetApi().AddSwaggerUI(), engine: engine);

        var request = host.GetRequest("/swagger");

        request.Method = HttpMethod.Post;
        request.Content = new StringContent("Content");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);

        Assert.AreEqual("GET", response.GetContentHeader("Allow"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResourceAccess(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(GetApi().AddSwaggerUI(), engine: engine);

        using var response = await host.GetResponseAsync("/swagger/static/swagger-ui.css");

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFound(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(GetApi().AddSwaggerUI(), engine: engine);

        using var response = await host.GetResponseAsync("/swagger/notfound");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomMeta(TestEngine engine)
    {
        var api = GetApi().AddSwaggerUI("docs", "https://localhost:5001/swagger.json", "My API");

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync("/docs/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var content = await response.GetContentAsync();

        AssertX.Contains("https://localhost:5001/swagger.json", content);
        AssertX.Contains("My API", content);
    }

    private static LayoutBuilder GetApi()
    {
        return Layout.Create()
                     .Add(Inline.Create().Get(() => 42))
                     .AddOpenApi();
    }

}
