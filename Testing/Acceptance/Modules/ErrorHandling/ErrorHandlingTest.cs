using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.ErrorHandling;

[TestClass]
public sealed class ErrorHandlingTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGenericError(ServerEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            throw new NotImplementedException();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEscaping(ServerEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            throw new Exception("Nah <>");
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        AssertX.DoesNotContain("<>", await response.GetContentAsync());
    }

}
