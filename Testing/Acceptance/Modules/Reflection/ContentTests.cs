using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ContentTests
{

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestDeserialization(ServerEngine engine, ExecutionMode mode)
    {
        var expectation = new MyType(42);

        var handler = Inline.Create()
                            .Get(() => expectation)
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(expectation, await response.GetContentAsync<MyType>());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNull(ServerEngine engine, ExecutionMode mode)
    {
        var handler = Inline.Create()
                            .Get(() => (MyType?)null)
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.IsNull(await response.GetOptionalContentAsync<MyType>());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestUnsupported(ServerEngine engine, ExecutionMode mode)
    {
        var handler = Inline.Create()
                            .Get(() => new Result<string>("Nah"))
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await response.GetOptionalContentAsync<MyType>();
        });
    }

    public record MyType(int Id);
}
