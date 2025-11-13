using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine.Internal;

[TestClass]
public class BasicTests
{
    
    [TestMethod]
    public async Task TestServerHeader()
    {
        await using var runner = await TestHost.RunAsync(Layout.Create(), engine: TestEngine.Internal);

        using var response = await runner.GetResponseAsync();

        Assert.StartsWith("GenHTTP/", response.GetHeader("Server"));
    }
    
}
