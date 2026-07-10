using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine.Compliance;

[TestClass]
public class ComplianceTests : WireTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoMinorHttpVersion(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["GET / HTTP/1", "Host: localhost"], "505", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNonSupportedVersion(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["GET / HTTP/1.2", "Host: localhost"], "505", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyHost(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["GET / HTTP/1.0", "Host: "], "400", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostNoBody(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["POST / HTTP/1.0", "Host: host"], "404", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetWithBody(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["GET / HTTP/1.0", "Host: host", "Content-Length: 3", "", "123"], "400", Layout.Create(), engine);
    }

}
