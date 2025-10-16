using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;

using Microsoft.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class ArgumentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInjectionCausesNoArgument(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get((IRequest request) => request.Host);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        AssertX.Empty(op.Parameters);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathParam(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/users/:id", (int id) => id);

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/users/{id}", path.Item1);
        AssertParameter(operation, "id", ParameterLocation.Path);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestQueryParam(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/users/", (int id) => id);

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/users/", path.Item1);
        AssertParameter(operation, "id", ParameterLocation.Query);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBodyParam(TestEngine engine)
    {
        var api = Inline.Create()
                        .Post("/users/filter", (HashSet<int> items) => items.Count);

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/users/filter", path.Item1);

        Assert.IsTrue(operation.RequestBody?.Content?.ContainsKey("application/json") ?? false);
        Assert.IsTrue(operation.RequestBody?.Content?.ContainsKey("text/xml") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentParam(TestEngine engine)
    {
        var api = Inline.Create()
                        .Post("/users/filter", ([FromBody] DateOnly date) => date);

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/users/filter", path.Item1);

        Assert.IsTrue(operation.RequestBody?.Content?.ContainsKey("text/plain") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStreamParam(TestEngine engine)
    {
        var api = Inline.Create()
                        .Put("/users/avatar", (Stream file) => true);

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/users/avatar", path.Item1);

        Assert.IsTrue(operation.RequestBody?.Content?.ContainsKey("*/*") ?? false);
        Assert.AreEqual("binary", operation.RequestBody?.Content?["*/*"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestWildcardParameter(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/files/:tenant/", () => Content.From(Resource.FromString("File Content")));

        var (path, operation) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("/files/{tenant}/{remainingPath}", path.Item1);
    }

    #region Helpers

    private static void AssertParameter(OpenApiOperation operation, string name, ParameterLocation location)
    {
        Assert.AreEqual(1, operation.Parameters?.Count);

        var param = operation.Parameters?.FirstOrDefault();

        Assert.IsNotNull(param);

        Assert.AreEqual(name, param.Name);
        Assert.AreEqual(location, param.In);
    }

    #endregion

}
