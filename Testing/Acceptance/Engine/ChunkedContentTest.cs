using System.Net;
using System.Net.Http.Json;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ChunkedContentTest
{

    #region Supporting data structures

    private record Model(string Value);

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestChunkedUpload()
    {
            var inline = Inline.Create()
                               .Put((Model model) => model);

            using var runner = TestHost.Run(inline);

            using var client = TestHost.GetClient();

            using var response = await client.PutAsJsonAsync<Model>(runner.GetUrl(), new("Hello World"));

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var result = await response.GetContentAsync();

            AssertX.Contains("Hello World", result);
        }

    #endregion

}
