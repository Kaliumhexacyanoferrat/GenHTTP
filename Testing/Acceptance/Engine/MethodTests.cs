using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class MethodTests
{

    [TestMethod]
    public async Task TestCustomMethods()
    {
            var result = Content.From(Resource.FromString("OK"));

            using var host = TestHost.Run(result);

            var request = host.GetRequest(method: new HttpMethod("BREW"));

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

}
