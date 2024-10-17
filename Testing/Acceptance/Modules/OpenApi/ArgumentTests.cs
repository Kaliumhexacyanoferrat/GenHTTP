using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class ArgumentTests
{

    [TestMethod]
    public async Task TestInjectionCausesNoArgument()
    {
        var api = Inline.Create()
                        .Get((IRequest request) => request.Host);

        var (_, op) = await Extensions.GetOperationAsync(api);

        AssertX.Empty(op.Parameters);
    }

    [TestMethod]
    public async Task TestPathParam()
    {
        var api = Inline.Create()
                        .Get("/users/:id", (int id) => id);

        var (path, operation) = await Extensions.GetOperationAsync(api);

        Assert.AreEqual("/users/{id}", path.Item1);
        AssertParameter(operation, "id", ParameterLocation.Path);
    }

    [TestMethod]
    public async Task TestQueryParam()
    {
        var api = Inline.Create()
                        .Get("/users/", (int id) => id);

        var (path, operation) = await Extensions.GetOperationAsync(api);

        Assert.AreEqual("/users/", path.Item1);
        AssertParameter(operation, "id", ParameterLocation.Query);
    }

    [TestMethod]
    public async Task TestBodyParam()
    {
        var api = Inline.Create()
                        .Post("/users/filter", (HashSet<int> items) => items.Count);

        var (path, operation) = await Extensions.GetOperationAsync(api);

        Assert.AreEqual("/users/filter", path.Item1);

        Assert.IsTrue(operation.RequestBody.Content.ContainsKey("application/json"));
        Assert.IsTrue(operation.RequestBody.Content.ContainsKey("text/xml"));
    }

    [TestMethod]
    public async Task TestContentParam()
    {
        var api = Inline.Create()
                        .Post("/users/filter", ([FromBody] DateOnly date) => date);

        var (path, operation) = await Extensions.GetOperationAsync(api);

        Assert.AreEqual("/users/filter", path.Item1);

        Assert.IsTrue(operation.RequestBody.Content.ContainsKey("text/plain"));
    }

    [TestMethod]
    public async Task TestStreamParam()
    {
        var api = Inline.Create()
                        .Put("/users/avatar", (Stream file) => true);

        var (path, operation) = await Extensions.GetOperationAsync(api);

        Assert.AreEqual("/users/avatar", path.Item1);

        Assert.IsTrue(operation.RequestBody.Content.ContainsKey("*/*"));
        Assert.AreEqual("binary", operation.RequestBody.Content["*/*"].Schema.Format);
    }

    #region Helpers

    private static void AssertParameter(OpenApiOperation operation, string name, ParameterLocation location)
    {
        Assert.AreEqual(1, operation.Parameters.Count);

        var param = operation.Parameters.First();

        Assert.AreEqual(name, param.Name);
        Assert.AreEqual(location, param.In);
    }

    #endregion

}
