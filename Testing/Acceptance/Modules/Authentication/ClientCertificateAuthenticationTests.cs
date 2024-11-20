using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.ClientCertificate;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public class ClientCertificateAuthenticationTests
{

    #region Supporting data structures

    public class ConfigurableValidator : ICertificateValidator
    {

        public Func<X509Certificate?, X509Chain?, SslPolicyErrors, bool>? Validator { get; set; }

        public bool RequireCertificate { get; set; }

        public X509RevocationMode RevocationCheck { get; set; }

        public bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors) => Validator?.Invoke(certificate, chain, policyErrors) ?? false;

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientAuthentication(TestEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create()
                                       .Authorization((_, c) => new(c != null));

        var validator = new ConfigurableValidator()
        {
            RequireCertificate = true,
            RevocationCheck = X509RevocationMode.NoCheck,
            Validator = (_, _, _) => true
        };

        using var response = await TestAsync(auth, validator, true, engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientAuthenticationSaysNo(TestEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create()
                                       .Authorization((_, _) => new(false));

        var validator = new ConfigurableValidator()
        {
            RequireCertificate = true,
            RevocationCheck = X509RevocationMode.NoCheck,
            Validator = (_, _, _) => true
        };

        using var response = await TestAsync(auth, validator, true, engine);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAuthenticationOptional(TestEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create()
                                       .Authorization((_, c) => new(c == null));

        var validator = new ConfigurableValidator()
        {
            RequireCertificate = false,
            RevocationCheck = X509RevocationMode.NoCheck,
            Validator = (_, _, _) => true
        };

        using var response = await TestAsync(auth, validator, false, engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUserMapping(TestEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create()
                                       .UserMapping((_, c) => new((c != null) ? new ClientCertificateUser(c) : null));

        var validator = new ConfigurableValidator()
        {
            RequireCertificate = true,
            RevocationCheck = X509RevocationMode.NoCheck,
            Validator = (_, _, _) => true
        };

        using var response = await TestAsync(auth, validator, true, engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("CN=genhttp.local, O=GenHTTP", await response.GetContentAsync());
    }

    #endregion

    #region Helpers

    private static async ValueTask<HttpResponseMessage> TestAsync(ClientCertificateAuthenticationBuilder auth, ICertificateValidator validator, bool sendCertificate, TestEngine engine)
    {
        var certificate = await Utilities.Security.GetCertificateAsync();

        var content = Inline.Create()
                                      .Get((IRequest request) => request.GetUser<IUser>()?.DisplayName ?? "No user")
                                      .Add(auth);

        var runner = new TestHost(content.Build(), engine: engine);

        var port = TestHost.NextPort();

        runner.Host
              .Handler(content)
              .Bind(IPAddress.Any, (ushort)port, certificate, certificateValidator: validator);

        await runner.StartAsync();

        using var client = await GetClientAsync(sendCertificate);

        return await client.GetAsync($"https://localhost:{port}");
    }

    private static async ValueTask<HttpClient> GetClientAsync(bool sendCertificate)
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        if (sendCertificate)
        {
            var certificate = await Utilities.Security.GetCertificateAsync();
            handler.ClientCertificates.Add(certificate);
        }

        return new HttpClient(handler);
    }

    #endregion

}
