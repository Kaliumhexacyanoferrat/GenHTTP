using System.Net;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class DataTests
{

    #region Controller

    public class TestController
    {

        [ControllerAction(RequestMethod.POST)]
        public DateOnly Date(DateOnly date) => date;

    }

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestDateOnly()
    {
            using var host = GetHost();

            var request = host.GetRequest("/t/date/", HttpMethod.Post);

            var data = new Dictionary<string, string>()
            {
                { "date", "2024-03-11" }
            };

            request.Content = new FormUrlEncodedContent(data);

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("2024-03-11", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestInvalidDateOnly()
    {
            using var host = GetHost();

            var request = host.GetRequest("/t/date/", HttpMethod.Post);

            var data = new Dictionary<string, string>()
            {
                { "date", "ABC" }
            };

            request.Content = new FormUrlEncodedContent(data);

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.BadRequest);
        }

    #endregion

    #region Helpers

    private static TestHost GetHost()
    {
            var app = Layout.Create()
                            .AddController<TestController>("t", serializers: Serialization.Default(),
                                                                injectors: Injection.Default(), 
                                                                formatters: Formatting.Default());

            return TestHost.Run(app);
        }

    #endregion

}
