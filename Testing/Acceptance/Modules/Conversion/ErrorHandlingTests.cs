using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public class ErrorHandlingTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task UndeserializableBodyReturnsWithBadRequest(TestEngine engine)
    {
        var inline = Inline.Create()
                           .Post("/t", (MyEntity entity) => entity.Data);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var request = runner.GetRequest("/t");

        request.Method = HttpMethod.Post;

        request.Content = new StringContent("I cannot be deserialized", null, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Supporting data structures

    private record MyEntity(string Data);

    #endregion

}
