using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Handler;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class MetaDataTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDefaultTitleAndVersion(TestEngine engine)
    {
        var doc = await GetApi(ApiDescription.Create()).GetOpenApiAsync(engine);

        Assert.AreEqual("Swagger specification", doc.Document?.Info.Title);
        Assert.AreEqual("1.0.0", doc.Document?.Info.Version);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomTitleAndVersion(TestEngine engine)
    {
        var desc = ApiDescription.Create()
                                 .Title("My Title")
                                 .Version("2.0.1");

        var doc = await GetApi(desc).GetOpenApiAsync(engine);

        Assert.AreEqual("My Title", doc.Document?.Info.Title);
        Assert.AreEqual("2.0.1", doc.Document?.Info.Version);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerGenerated(TestEngine engine)
    {
        var doc = await GetApi(ApiDescription.Create()).GetOpenApiAsync(engine);

        var server = doc.Document?.Servers?.First();

        AssertX.Contains("http://localhost:", server?.Url);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostProcessing(TestEngine engine)
    {
        var desc = ApiDescription.Create()
                                 .PostProcessor((_, doc) => doc.Servers.First().Url = "https://google.de/");

        var doc = await GetApi(desc).GetOpenApiAsync(engine);

        Assert.AreEqual("https://google.de/", doc.Document?.Servers?.First().Url);
    }

    private static InlineBuilder GetApi(OpenApiConcernBuilder description)
    {
        return Inline.Create()
                     .Get(() => "Hello World")
                     .Add(description);
    }
}
