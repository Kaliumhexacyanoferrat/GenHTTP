using System.Net;
using System.Net.Http.Json;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ChunkedContentTest
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestChunkedUpload(TestEngine engine)
    {
        var inline = Inline.Create()
                           .Put((Model model) => model);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var client = TestHost.GetClient();

        using var response = await client.PutAsJsonAsync(runner.GetUrl(), new Model("Hello World"));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var result = await response.GetContentAsync();

        AssertX.Contains("Hello World", result);
    }

    #endregion

    #region Supporting data structures

    private record Model(string Value);

    #endregion

}
