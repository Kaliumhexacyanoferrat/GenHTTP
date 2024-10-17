using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Handler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class MetaDataTests
{

    [TestMethod]
    public async Task TestDefaultTitleAndVersion()
    {
        var doc = await GetApi(ApiDescription.Create()).GetOpenApiAsync();

        Assert.AreEqual("Swagger specification", doc.OpenApiDocument.Info.Title);
        Assert.AreEqual("1.0.0", doc.OpenApiDocument.Info.Version);
    }

    [TestMethod]
    public async Task TestCustomTitleAndVersion()
    {
        var desc = ApiDescription.Create()
                                 .Title("My Title")
                                 .Version("2.0.1");

        var doc = await GetApi(desc).GetOpenApiAsync();

        Assert.AreEqual("My Title", doc.OpenApiDocument.Info.Title);
        Assert.AreEqual("2.0.1", doc.OpenApiDocument.Info.Version);
    }

    [TestMethod]
    public async Task TestServerGenerated()
    {
        var doc = await GetApi(ApiDescription.Create()).GetOpenApiAsync();

        var server = doc.OpenApiDocument.Servers.First();

        AssertX.Contains("http://localhost:", server.Url);
    }

    [TestMethod]
    public async Task TestPostProcessing()
    {
        var desc = ApiDescription.Create()
                                 .PostProcessor((_, doc) => doc.Servers.First().Url = "https://google.de/");

        var doc = await GetApi(desc).GetOpenApiAsync();

        Assert.AreEqual("https://google.de/", doc.OpenApiDocument.Servers.First().Url);
    }

    private static InlineBuilder GetApi(OpenApiConcernBuilder description)
    {
        return Inline.Create()
                     .Get(() => "Hello World")
                     .Add(description);
    }
}
