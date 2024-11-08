using System.Net;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class FormatTests
{

    [TestMethod]
    [MultiEngineTest]
    public Task TestJsonFile(TestEngine engine) => TestApi(engine, "/openapi.json", "application/json");

    [TestMethod]
    [MultiEngineTest]
    public Task TestJsonFileUpper(TestEngine engine) => TestApi(engine, "/OPENAPI.JSON", "application/json");

    [TestMethod]
    [MultiEngineTest]
    public Task TestJsonFallback(TestEngine engine) => TestApi(engine, "/openapi", "application/json");

    [TestMethod]
    [MultiEngineTest]
    public Task TestYamlFile(TestEngine engine) => TestApi(engine, "/openapi.yaml", "application/yaml");

    [TestMethod]
    [MultiEngineTest]
    public Task TestYmlFile(TestEngine engine) => TestApi(engine, "/openapi.yml", "application/yaml");

    [TestMethod]
    [MultiEngineTest]
    public Task TestJsonByAccept(TestEngine engine) => TestApi(engine, "/openapi", accept: "application/json");

    [TestMethod]
    [MultiEngineTest]
    public Task TestJsonByAcceptUpper(TestEngine engine) => TestApi(engine, "/OPENAPI", accept: "application/json");

    [TestMethod]
    [MultiEngineTest]
    public Task TestYamlByAccept(TestEngine engine) => TestApi(engine, "/openapi", accept: "application/yaml");

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnsupportedFormat(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(GetApi(), engine: engine);

        var request = host.GetRequest("/openapi");

        request.Headers.Add("Accept", "text/plain");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    private static async Task TestApi(TestEngine engine, string path, string? expectedContentType = null, string? accept = null)
    {
        await using var host = await TestHost.RunAsync(GetApi(), engine: engine);

        var request = host.GetRequest(path);

        if (accept != null)
        {
            request.Headers.Add("Accept", accept);
        }

        using var response = await host.GetResponseAsync(request);

        Assert.AreEqual(expectedContentType ?? accept, response.GetContentHeader("Content-Type"));

        await response.AsOpenApiAsync();
    }

    private static InlineBuilder GetApi()
    {
        return Inline.Create()
                     .Get(() => "Hello World")
                     .Add(ApiDescription.Create());
    }
}
