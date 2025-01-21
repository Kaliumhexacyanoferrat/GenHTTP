using System.Net;

using GenHTTP.Modules.ServerSentEvents;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ServerSentEvents;

[TestClass]
public sealed class DataTests
{
    private const string NL = "\n";

    #region Supporting data structures

    private record MyType(string One, int Two);

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public Task TestString(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync("my string"), "my string");

    [TestMethod]
    [MultiEngineTest]
    public Task TestInt(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync(42), "42");

    [TestMethod]
    [MultiEngineTest]
    public Task TestDate(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync(DateOnly.FromDayNumber(8445)), "0024-02-15");

    [TestMethod]
    [MultiEngineTest]
    public Task TestComplex(TestEngine engine) => TestAsync(engine, async c => await c.DataAsync(new MyType("1", 2)), "{\"one\":\"1\",\"two\":2}");

    private static async Task TestAsync(TestEngine engine, Func<IEventConnection, ValueTask> generator, string expected)
    {
        var source = EventSource.Create()
                                .Generator(generator);

        await using var host = await TestHost.RunAsync(source, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual($"data: {expected}{NL}{NL}", await response.GetContentAsync());
    }

    #endregion

}
