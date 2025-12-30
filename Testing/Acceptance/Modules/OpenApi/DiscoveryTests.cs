using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Discovery;
using System.Linq;
using NSwag;
using OpenApiDocument = NSwag.OpenApiDocument;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class DiscoveryTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTraversal(TestEngine engine)
    {
        var api = Layout.Create()
                        .Add("service", Layout.Create().Add(Inline.Create().Get("/method", () => 1)))
                        .Add(Inline.Create().Get("/method2", () => 2))
                        .AddRangeSupport()
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.IsTrue(doc?.Paths.ContainsKey("/service/method") ?? false);
        Assert.IsTrue(doc?.Paths.ContainsKey("/method2") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomExplorer(TestEngine engine)
    {
        var discovery = ApiDiscovery.Empty().Add<CustomExplorer>();

        var api = Inline.Create().Add(ApiDescription.With(discovery));

        var doc = (await api.GetOpenApiAsync(engine, false)).Document;

        Assert.AreEqual("Added by explorer", doc?.Servers?.First().Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSamePathWithDifferentMethods(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/method", () => 42)
                        .Put("/method", (int i) => i)
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        Assert.AreEqual(1, doc.Paths?.Count ?? 0);

        var operations = doc.Paths?.First().Value.Operations;

        Assert.AreEqual(2, operations?.Count ?? 0);

        Assert.IsTrue(operations?.ContainsKey(HttpMethod.Get));
        Assert.IsTrue(operations?.ContainsKey(HttpMethod.Put));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestApiKeyAuthenticationIsReflected(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/secure", () => 42)
                        .Add(ApiDescription.Create()
                                           .WithApiKeyAuthentication(
                                               schemeName: "X-API-Key",
                                               headerName: "X-API-Key",
                                               includePath: _ => true));

        var result = await api.GetOpenApiAsync(engine);
        var doc = result.Document;
        Assert.IsNotNull(doc, "OpenAPI document should not be null.");

        var paths = doc!.Paths;
        Assert.IsNotNull(paths, "Paths collection should not be null.");

        Assert.IsTrue(paths!.TryGetValue("/secure", out var pathItem),
                      "Path '/secure' should be present.");
        Assert.IsNotNull(pathItem, "Path item for '/secure' should not be null.");

        var operations = pathItem!.Operations;
        Assert.IsNotNull(operations, "Operations collection should not be null.");

        Assert.IsTrue(operations!.TryGetValue(HttpMethod.Get, out var operation),
                      "GET operation for '/secure' should be present.");
        Assert.IsNotNull(operation, "Operation for GET '/secure' should not be null.");
        var responses = operation!.Responses;
        Assert.IsNotNull(responses, "Responses collection should not be null.");
        Assert.IsTrue(responses!.ContainsKey("401"),
                      "401 Unauthorized response should be documented.");

        var security = operation.Security;
        Assert.IsNotNull(security, "Security requirements should be set on the operation.");
        Assert.IsTrue(security!.Any(),
                      "There should be at least one security requirement.");
    }

    #region Supporting data structures

    public class CustomExplorer : IApiExplorer
    {

        public bool CanExplore(IHandler handler) => true;

        public ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
        {
            document.Servers.First().Description = "Added by explorer";

            return ValueTask.CompletedTask;
        }
    }

    #endregion

}
