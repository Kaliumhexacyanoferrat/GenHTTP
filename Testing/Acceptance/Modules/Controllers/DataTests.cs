using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class DataTests
{

    #region Helpers

    private static async Task<TestHost> GetHostAsync(TestEngine engine, ExecutionMode mode)
    {
        var app = Layout.Create()
                        .AddController<TestController>("t", serializers: Serialization.Default(),
                                                       injectors: Injection.Default(),
                                                       formatters: Formatting.Default(),
                                                       mode: mode);

        return await TestHost.RunAsync(app, engine: engine);
    }

    #endregion

    #region Controller

    public class TestController
    {

        [ControllerAction(RequestMethod.Post)]
        public DateOnly Date(DateOnly date) => date;
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestDateOnly(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await GetHostAsync(engine, mode);

        var request = host.GetRequest("/t/date/", HttpMethod.Post);

        var data = new Dictionary<string, string>
        {
            {
                "date", "2024-03-11"
            }
        };

        request.Content = new FormUrlEncodedContent(data);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("2024-03-11", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestInvalidDateOnly(TestEngine engine, ExecutionMode mode)
    {
        await using var host = await GetHostAsync(engine, mode);

        var request = host.GetRequest("/t/date/", HttpMethod.Post);

        var data = new Dictionary<string, string>
        {
            {
                "date", "ABC"
            }
        };

        request.Content = new FormUrlEncodedContent(data);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    #endregion

}
