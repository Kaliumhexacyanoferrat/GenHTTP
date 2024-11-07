using System.Net;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ErrorHandlingTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGenericError(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            throw new NotImplementedException();
        });

        using var runner = TestHost.Run(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEscaping(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r =>
        {
            throw new Exception("Nah <>");
        });

        using var runner = TestHost.Run(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        AssertX.DoesNotContain("<>", await response.GetContentAsync());
    }

}
