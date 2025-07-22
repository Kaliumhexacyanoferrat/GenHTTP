using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class ResponseTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormatted(TestEngine engine)
    {
        var api = Inline.Create().Get(() => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("text/plain"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormattedNullable(TestEngine engine)
    {
        var api = Inline.Create().Get(() => (int?)42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("204"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStream(TestEngine engine)
    {
        var api = Inline.Create().Get(() => new MemoryStream());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("application/octet-stream"));

        Assert.AreEqual("binary", op.Responses?["200"].Content?["application/octet-stream"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNone(TestEngine engine)
    {
        var api = Inline.Create().Get(() => { });

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsFalse(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?.ContainsKey("204"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDynamic(TestEngine engine)
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
    public async Task TestSerialized(TestEngine engine)
    {
        var api = Inline.Create().Get(() => new HashSet<int>());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
        Assert.IsTrue(op.Responses?["200"].Content?.ContainsKey("application/json"));
    }

}
