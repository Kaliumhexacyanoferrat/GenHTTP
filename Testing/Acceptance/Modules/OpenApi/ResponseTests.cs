using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Redirects;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class ResponseTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormatted(ServerEngine engine)
    {
        var api = Inline.Create().Get(() => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("text/plain"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormattedNullable(ServerEngine engine)
    {
        var api = Inline.Create().Get(() => (int?)42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("204"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStream(ServerEngine engine)
    {
        var api = Inline.Create().Get(() => new MemoryStream());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("application/octet-stream"));

        Assert.AreEqual("binary", op.Responses?["200"].Content?["application/octet-stream"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNone(ServerEngine engine)
    {
        var api = Inline.Create().Get(() => { });

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsFalse(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?.ContainsKey("204"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDynamic(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("h", (IHandler parent) => Redirect.To("https://google.de").Build())
                        .Get("hb", () => Redirect.To("https://google.de"))
                        .Get("r", (IRequest request) => request.Respond().Build())
                        .Get("rb", (IRequest request) => request.Respond());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("*/*"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSerialized(ServerEngine engine)
    {
        var api = Inline.Create().Get(() => new HashSet<int>());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("application/json"));
    }

}
