using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class ProtocolTests
{

    [TestMethod]
    public async Task TestHttp2And3()
    {
        var logic = Inline.Create().Get((IRequest request) => request.ProtocolType);

        var client = GetClient();

        var (host, url) = await GetHostAsync(logic);

        try
        {
            using var response = await client.GetAsync(url);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("Http2", await response.GetContentAsync());

            Assert.IsTrue(response.Headers.Contains("Alt-Svc"));

            AssertX.Contains("h3", response.Headers.GetValues("Alt-Svc").First());
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

    #region Helpers

    private static HttpClient GetClient()
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        return new HttpClient(handler)
        {
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };
    }

    private static async Task<(TestHost, string)> GetHostAsync(IHandlerBuilder handler)
    {
        var certificate = await Utilities.Security.GetCertificateAsync();

        var runner = new TestHost(handler.Build(), engine: TestEngine.Kestrel);

        var port = TestHost.NextPort();

        runner.Host.Bind(IPAddress.Any, (ushort)port, certificate, enableQuic: true);

        await runner.StartAsync();

        return (runner, $"https://localhost:{port}");
    }

    #endregion

}
