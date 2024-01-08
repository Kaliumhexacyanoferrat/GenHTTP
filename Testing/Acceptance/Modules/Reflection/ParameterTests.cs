using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection
{

    [TestClass]
    public sealed class ParameterTests
    {

        #region Tests
        [TestMethod]
        public async Task TestCanReadSimpleTypesFromBody()
        {
            var inline = Inline.Create()
                               .Post(([FromBody] string body) => body);

            using var runner = TestHost.Run(inline);

            var request = runner.GetRequest();

            request.Method = HttpMethod.Post;
            request.Content = new StringContent("my body", null, "text/plain");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("my body", await response.GetContentAsync());
        }

        #endregion

    }

}
