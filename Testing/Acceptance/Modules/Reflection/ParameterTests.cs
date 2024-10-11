using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ParameterTests
{

    #region Tests

    [TestMethod]
    public async Task TestCanReadSimpleTypesFromBody()
    {
            var inline = Inline.Create()
                               .Post(([FromBody] string body1, [FromBody] string body2) => $"{body1}-{body2}");

            using var runner = TestHost.Run(inline);

            using var response = await PostAsync(runner, "1");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("1-1", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestCanPassEmptyString()
    {
            var inline = Inline.Create()
                               .Post(([FromBody] int number) => number);

            using var runner = TestHost.Run(inline);

            using var response = await PostAsync(runner, " ");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("0", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestCanAccessBothBodyAndStream()
    {
            var inline = Inline.Create()
                               .Post(([FromBody] int number, Stream body) =>
                               {
                                   using var reader = new StreamReader(body);
                                   return $"{number} - {reader.ReadToEnd()}";
                               });

            using var runner = TestHost.Run(inline);

            using var response = await PostAsync(runner, "1");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("1 - 1", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestConversionError()
    {
            var inline = Inline.Create()
                               .Post(([FromBody] int number) => number);

            using var runner = TestHost.Run(inline);

            using var response = await PostAsync(runner, "ABC");

            await response.AssertStatusAsync(HttpStatusCode.BadRequest);
        }

    private static Task<HttpResponseMessage> PostAsync(TestHost host, string body)
    {
            var request = host.GetRequest();

            request.Method = HttpMethod.Post;
            request.Content = new StringContent(body, null, "text/plain");

            return host.GetResponseAsync(request);
        }

    #endregion

}
