using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class BearerAuthenticationTests
{
    private const string ValidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidToken(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth, engine, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomValidator(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .Validation(_ => throw new ProviderException(ResponseStatus.Forbidden, "Nah"))
                                       .AllowExpired();

        using var response = await Execute(auth, engine, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomKeyResolver(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .Issuer("https://facebook.com")
                                       .KeyResolver(_ => throw new ProviderException(ResponseStatus.Forbidden, "Nah"))
                                       .AllowExpired();

        using var response = await Execute(auth, engine, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoUser(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .UserMapping((_, _) => new ValueTask<IUser?>())
                                       .AllowExpired();

        using var response = await Execute(auth, engine, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUser(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .UserMapping((_, _) => new ValueTask<IUser?>(new MyUser
                                       {
                                           DisplayName = "User Name"
                                       }))
                                       .AllowExpired();

        using var response = await Execute(auth, engine, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoToken(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth, engine);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMalformedToken(TestEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth, engine, "Lorem Ipsum");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIssuerFetchesRealSigningKeys(TestEngine engine)
    {
        using var rsa = RSA.Create(2048);

        await using var issuerHost = await CreateIssuerAsync(engine, rsa);

        var issuer = issuerHost.GetUrl();

        var auth = BearerAuthentication.Create().Issuer(issuer).AllowExpired();

        var token = CreateToken(issuer, rsa);

        using var response = await Execute(auth, engine, token);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIssuerRejectsTokenSignedWithUnknownKey(TestEngine engine)
    {
        using var rsa = RSA.Create(2048);
        using var otherRsa = RSA.Create(2048);

        await using var issuerHost = await CreateIssuerAsync(engine, rsa);

        var issuer = issuerHost.GetUrl();

        var auth = BearerAuthentication.Create().Issuer(issuer);

        var token = CreateToken(issuer, otherRsa);

        using var response = await Execute(auth, engine, token);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnreachableIssuerConfigYieldsInternalServerError(TestEngine engine)
    {
        // no ".well-known/openid-configuration" route configured -> the issuer 404s
        await using var issuerHost = await TestHost.RunAsync(Layout.Create(), engine: engine);

        var issuer = issuerHost.GetUrl();

        var auth = BearerAuthentication.Create().Issuer(issuer).AllowExpired();

        using var rsa = RSA.Create(2048);

        var token = CreateToken(issuer, rsa);

        using var response = await Execute(auth, engine, token);

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

    private static async Task<TestHost> CreateIssuerAsync(TestEngine engine, RSA rsa)
    {
        var securityKey = new RsaSecurityKey(rsa) { KeyId = "test-key" };
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);

        var jwksJson = $$"""{"keys":[{"kty":"RSA","use":"sig","kid":"{{jwk.Kid}}","n":"{{jwk.N}}","e":"{{jwk.E}}","alg":"RS256"}]}""";

        // the handler references the host's own port (for jwks_uri), so it's built after the
        // port is known but before the not-yet-started host is actually started
        var issuerHost = new TestHost(Layout.Create().Build(), engine: engine);

        var configJson = $$"""{"jwks_uri":"{{issuerHost.GetUrl("/jwks")}}"}""";

        issuerHost.Host.Handler(Layout.Create()
                                       .Add(".well-known", Layout.Create()
                                                                  .Add("openid-configuration", Inline.Create().Get(() => configJson)))
                                       .Add("jwks", Inline.Create().Get(() => jwksJson)));

        await issuerHost.StartAsync();

        return issuerHost;
    }

    private static string CreateToken(string issuer, RSA rsa)
    {
        var handler = new JwtSecurityTokenHandler();

        var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(issuer: issuer, claims: [], signingCredentials: credentials);

        return handler.WriteToken(token);
    }

    private static async Task<HttpResponseMessage> Execute(BearerAuthenticationConcernBuilder builder, TestEngine engine, string? token = null)
    {
        var handler = Inline.Create()
                            .Get(() => "Secured")
                            .Add(builder);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        var request = host.GetRequest();

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await host.GetResponseAsync(request);
    }

    #region Supporting data structures

    internal class MyUser : IUser
    {

        public string DisplayName { get; init; } = "";
    }

    #endregion

}
