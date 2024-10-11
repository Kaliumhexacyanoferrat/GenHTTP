using System.Net;
using System.Threading.Tasks;

using GenHTTP.Api.Content;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class AmbiguityTests
{

    #region Supporting data structures

    public sealed class TestService
    {

        [ResourceMethod]
        public IHandlerBuilder Wildcard()
        {
                return Content.From(Resource.FromString("Wildcard"));
            }

        [ResourceMethod(path: "/my.txt")]
        public string Specific() => "Specific";

    }

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestSpecificPreferred()
    {
            var app = Layout.Create()
                            .AddService<TestService>("c");

            using var host = TestHost.Run(app);

            using var response = await host.GetResponseAsync("/c/my.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("Specific", await response.GetContentAsync());
        }

    #endregion

}