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
    public Task TestJsonFile() => TestApi("/openapi.json", "application/json");

    [TestMethod]
    public Task TestJsonFileUpper() => TestApi("/OPENAPI.JSON", "application/json");

    [TestMethod]
    public Task TestJsonFallback() => TestApi("/openapi", "application/json");

    [TestMethod]
    public Task TestYamlFile() => TestApi("/openapi.yaml", "application/yaml");

    [TestMethod]
    public Task TestYmlFile() => TestApi("/openapi.yml", "application/yaml");

    [TestMethod]
    public Task TestJsonByAccept() => TestApi("/openapi", accept: "application/json");

    [TestMethod]
    public Task TestJsonByAcceptUpper() => TestApi("/OPENAPI", accept: "application/json");

    [TestMethod]
    public Task TestYamlByAccept() => TestApi("/openapi", accept: "application/yaml");

    [TestMethod]
    public async Task TestUnsupportedFormat()
    {
        using var host = TestHost.Run(GetApi());

        var request = host.GetRequest("/openapi");

        request.Headers.Add("Accept", "text/plain");

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    private static async Task TestApi(string path, string? expectedContentType = null, string? accept = null)
    {

        using var host = TestHost.Run(GetApi());

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
