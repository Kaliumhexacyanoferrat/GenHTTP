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
    public Task TestString() => TestAsync(async (c) => await c.DataAsync("my string"), "my string");

    [TestMethod]
    public Task TestInt() => TestAsync(async (c) => await c.DataAsync(42), "42");

    [TestMethod]
    public Task TestDate() => TestAsync(async (c) => await c.DataAsync(DateOnly.FromDayNumber(8445)), "0024-02-15");

    [TestMethod]
    public Task TestComplex() => TestAsync(async (c) => await c.DataAsync(new MyType("1", 2)), "{\"one\":\"1\",\"two\":2}");

    private static async Task TestAsync(Func<IEventConnection, ValueTask> generator, string expected)
    {
        var source = EventSource.Create()
                                .Generator(generator);

        await using var host = await TestHost.RunAsync(source);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual($"data: {expected}{NL}{NL}", await response.GetContentAsync());
    }

    #endregion

}
