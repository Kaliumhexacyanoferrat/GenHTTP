using GenHTTP.Api.Content;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Discovery;
using GenHTTP.Modules.OpenApi.Handler;

using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenApiDocument = NSwag.OpenApiDocument;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class DiscoveryTests
{

    [TestMethod]
    public async Task TestTraversal()
    {
        var api = Layout.Create()
                        .Add("service", Layout.Create().Add(Inline.Create().Get("/method", () => 1)))
                        .Add(Inline.Create().Get("/method2", () => 2))
                        .AddRangeSupport()
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.IsTrue(doc.Paths.ContainsKey("/service/method"));
        Assert.IsTrue(doc.Paths.ContainsKey("/method2"));
    }

    [TestMethod]
    public async Task TestCustomExplorer()
    {
        var discovery = ApiDiscovery.Empty().Add<CustomExplorer>();

        var api = Inline.Create().Add(ApiDescription.With(discovery));

        var doc = (await api.GetOpenApiAsync(false)).OpenApiDocument;

        Assert.AreEqual("Added by explorer", doc.Servers.First().Description);
    }

    [TestMethod]
    public async Task TestSamePathWithDifferentMethods()
    {
        var api = Inline.Create()
                        .Get("/method", () => 42)
                        .Put("/method", (int i) => i)
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.AreEqual(1, doc.Paths.Count);

        var operations = doc.Paths.First().Value.Operations;

        Assert.AreEqual(2, operations.Count);

        Assert.IsTrue(operations.ContainsKey(OperationType.Get));
        Assert.IsTrue(operations.ContainsKey(OperationType.Put));
    }

    #region Supporting data structures

    public class CustomExplorer : IApiExplorer
    {

        public bool CanExplore(IHandler handler) => true;

        public void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
        {
            document.Servers.First().Description = "Added by explorer";
        }
    }

    #endregion

}
