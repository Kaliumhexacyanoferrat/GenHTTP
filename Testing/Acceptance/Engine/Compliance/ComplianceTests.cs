using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine.Compliance;

[TestClass]
public class ComplianceTests : WireTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyHost(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        await TestAsync(["GET / HTTP/1.1", "Host: "], "400", Layout.Create(), engine);
    }

}
