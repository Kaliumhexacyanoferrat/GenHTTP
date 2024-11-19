using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Client;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public class ClientAuthenticationTests
{

    #region Supporting data structures

    public class ConfigurableValidator : ICertificateValidator
    {

        public Func<X509Certificate?, X509Chain?, SslPolicyErrors, bool>? Validator { get; set; }

        public bool ForceClientCertificate { get; set; }

        public X509RevocationMode RevocationMode { get; set; }

        public bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors) => Validator?.Invoke(certificate, chain, policyErrors) ?? false;

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientAuthentication(TestEngine engine)
    {
        var auth = ClientAuthentication.Create()
                                       .Authentication((_, c) => new(c != null));

        var validator = new ConfigurableValidator()
        {
            ForceClientCertificate = true,
            RevocationMode = X509RevocationMode.NoCheck,
            Validator = (_, _, _) => true
        };

        using var response = await TestAsync(auth, validator, engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    #endregion

    #region Helpers

    private static async ValueTask<HttpResponseMessage> TestAsync(ClientAuthenticationBuilder auth, ICertificateValidator validator, TestEngine engine)
    {
        var certificate = await Utilities.Security.GetCertificateAsync();

        var content = Content.From(Resource.FromString("Authenticated"))
                             .Add(auth);

        var runner = new TestHost(content.Build(), engine: engine);

        var port = TestHost.NextPort();

        runner.Host
              .Handler(content)
              .Bind(IPAddress.Any, (ushort)port, certificate, certificateValidator: validator);

        await runner.StartAsync();

        using var client = await GetClientAsync();

        return await client.GetAsync($"https://localhost:{port}");
    }

    private static async ValueTask<HttpClient> GetClientAsync()
    {
        var certificate = await Utilities.Security.GetCertificateAsync();

        return new HttpClient(new HttpClientHandler()
        {
            ClientCertificates =
            {
                certificate
            },
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });
    }

    #endregion

}
