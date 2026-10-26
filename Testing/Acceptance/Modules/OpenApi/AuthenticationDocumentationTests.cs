using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Content.Documentation.Security;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Handler;

using Microsoft.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public sealed class AuthenticationDocumentationTests
{

    #region API keys

    [TestMethod]
    [MultiEngineTest]
    public async Task TestApiKeyDefault(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Authentication(ApiKeyAuthentication.Create().Keys("123"));

        var doc = await GetDocumentAsync(api, engine);

        var scheme = doc.Components?.SecuritySchemes?["apiKey"];

        Assert.AreEqual(SecuritySchemeType.ApiKey, scheme?.Type);
        Assert.AreEqual(ParameterLocation.Header, scheme?.In);
        Assert.AreEqual("X-API-Key", scheme?.Name);

        var op = GetOperations(doc).Single();

        Assert.AreEqual("apiKey", GetSecurity(op.Security));

        Assert.IsTrue(op.Responses?.ContainsKey("401"));
        Assert.IsTrue(op.Responses?.ContainsKey("403"));
        Assert.IsTrue(op.Responses?.ContainsKey("200"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestApiKeyFromHeader(ServerEngine engine)
    {
        var scheme = await GetSchemeAsync(ApiKeyAuthentication.Create().WithHeader("X-Token").Keys("123"), engine);

        Assert.AreEqual(ParameterLocation.Header, scheme.In);
        Assert.AreEqual("X-Token", scheme.Name);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestApiKeyFromQuery(ServerEngine engine)
    {
        var scheme = await GetSchemeAsync(ApiKeyAuthentication.Create().WithQueryParameter("key").Keys("123"), engine);

        Assert.AreEqual(ParameterLocation.Query, scheme.In);
        Assert.AreEqual("key", scheme.Name);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestApiKeyFromCookie(ServerEngine engine)
    {
        var scheme = await GetSchemeAsync(ApiKeyAuthentication.Create().WithCookie("session").Keys("123"), engine);

        Assert.AreEqual(ParameterLocation.Cookie, scheme.In);
        Assert.AreEqual("session", scheme.Name);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomExtractorIsNotDocumented(ServerEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Extractor(r => r.Header.Headers.GetEntry("X-Custom"))
                                       .Keys("123");

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Authentication(auth), engine);

        var op = GetOperations(doc).Single();

        Assert.IsNull(op.Security);
        Assert.IsFalse(doc.Components?.SecuritySchemes?.Count > 0);

        Assert.IsTrue(op.Responses?.ContainsKey("401"));
        Assert.IsTrue(op.Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomExtractorWithScheme(ServerEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Extractor(r => r.Header.Headers.GetEntry("Authorization")?.Replace("Token ", ""))
                                       .Keys("123")
                                       .Documentation(SecurityScheme.ApiKey("Authorization", ApiKeyLocation.Header, "Send as 'Token <key>'"));

        var scheme = await GetSchemeAsync(auth, engine);

        Assert.AreEqual("Authorization", scheme.Name);
        Assert.AreEqual("Send as 'Token <key>'", scheme.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSchemeWinsOverExtractorRegardlessOfOrder(ServerEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Documentation(SecurityScheme.ApiKey("custom", ApiKeyLocation.Cookie))
                                       .WithHeader("X-Token")
                                       .Keys("123");

        var scheme = await GetSchemeAsync(auth, engine);

        Assert.AreEqual("custom", scheme.Name);
        Assert.AreEqual(ParameterLocation.Cookie, scheme.In);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDescription(ServerEngine engine)
    {
        var scheme = await GetSchemeAsync(ApiKeyAuthentication.Create().Keys("123").Documentation("Request a key from support"), engine);

        Assert.AreEqual("X-API-Key", scheme.Name);
        Assert.AreEqual("Request a key from support", scheme.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDescriptionOverridesScheme(ServerEngine engine)
    {
        var first = ApiKeyAuthentication.Create()
                                        .Keys("123")
                                        .Documentation("Overridden")
                                        .Documentation(SecurityScheme.ApiKey("X-Key", description: "Original"));

        var second = ApiKeyAuthentication.Create()
                                         .Keys("123")
                                         .Documentation(SecurityScheme.ApiKey("X-Key", description: "Original"))
                                         .Documentation("Overridden");

        Assert.AreEqual("Overridden", (await GetSchemeAsync(first, engine)).Description);
        Assert.AreEqual("Overridden", (await GetSchemeAsync(second, engine)).Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRenamedScheme(ServerEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Keys("123")
                                       .Documentation(SecurityScheme.ApiKey("X-Partner-Key") with { Name = "partnerKey" });

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Authentication(auth), engine);

        Assert.AreEqual("X-Partner-Key", doc.Components?.SecuritySchemes?["partnerKey"].Name);
        Assert.AreEqual("partnerKey", GetSecurity(GetOperations(doc).Single().Security));
    }

    #endregion

    #region Basic

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBasic(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Authentication((string _, string _) => new ValueTask<IUser?>());

        var doc = await GetDocumentAsync(api, engine);

        var scheme = doc.Components?.SecuritySchemes?["basic"];

        Assert.AreEqual(SecuritySchemeType.Http, scheme?.Type);
        Assert.AreEqual("basic", scheme?.Scheme);

        var op = GetOperations(doc).Single();

        Assert.AreEqual("basic", GetSecurity(op.Security));
        Assert.IsTrue(op.Responses?["401"].Headers?.ContainsKey("WWW-Authenticate"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBasicWithKnownUsers(ServerEngine engine)
    {
        var auth = BasicAuthentication.Create()
                                      .Add("bob", "secret")
                                      .Documentation("Use your intranet credentials");

        var scheme = await GetSchemeAsync(auth, engine);

        Assert.AreEqual("basic", scheme.Scheme);
        Assert.AreEqual("Use your intranet credentials", scheme.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBasicAsCustomHttpScheme(ServerEngine engine)
    {
        var auth = BasicAuthentication.Create((_, _) => new ValueTask<IUser?>())
                                      .Documentation(SecurityScheme.Http("Digest"));

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        Assert.AreEqual("Digest", doc.Components?.SecuritySchemes?["digest"].Scheme);
    }

    #endregion

    #region Bearer

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBearer(ServerEngine engine)
    {
        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(BearerAuthentication.Create()), engine);

        var scheme = doc.Components?.SecuritySchemes?["bearer"];

        Assert.AreEqual(SecuritySchemeType.Http, scheme?.Type);
        Assert.AreEqual("bearer", scheme?.Scheme);
        Assert.AreEqual("JWT", scheme?.BearerFormat);

        var op = GetOperations(doc).Single();

        Assert.AreEqual("bearer", GetSecurity(op.Security));
        Assert.IsTrue(op.Responses?.ContainsKey("400"));
        Assert.IsTrue(op.Responses?.ContainsKey("401"));
        Assert.IsFalse(op.Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBearerWithValidation(ServerEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .Validation(_ => Task.CompletedTask);

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        Assert.IsTrue(GetOperations(doc).Single().Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBearerWithOpenIdConnect(ServerEngine engine)
    {
        var auth = BearerAuthentication.Create()
                                       .Documentation(SecurityScheme.OpenIdConnect("https://sso.example.com/.well-known/openid-configuration"))
                                       .Documentation("Log in with your company account");

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        var scheme = doc.Components?.SecuritySchemes?["openIdConnect"];

        Assert.AreEqual(SecuritySchemeType.OpenIdConnect, scheme?.Type);
        Assert.AreEqual(new Uri("https://sso.example.com/.well-known/openid-configuration"), scheme?.OpenIdConnectUrl);
        Assert.AreEqual("Log in with your company account", scheme?.Description);

        Assert.AreEqual("openIdConnect", GetSecurity(GetOperations(doc).Single().Security));
    }

    #endregion

    #region Client certificates

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientCertificate(ServerEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create();

        var op = GetOperations(await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine)).Single();

        Assert.IsNull(op.Security);
        Assert.IsFalse(op.Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClientCertificateWithAuthorization(ServerEngine engine)
    {
        var auth = ClientCertificateAuthentication.Create()
                                                  .Authorization((_, _) => new ValueTask<bool>(true))
                                                  .Documentation(SecurityScheme.Http("mutual", "Present a client certificate"));

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        var op = GetOperations(doc).Single();

        Assert.AreEqual("mutual", GetSecurity(op.Security));
        Assert.IsTrue(op.Responses?.ContainsKey("403"));
    }

    #endregion

    #region Multiple mechanisms

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultiAuthenticationOffersAlternatives(ServerEngine engine)
    {
        var auth = MultiAuthentication.Create()
                                      .Add(ApiKeyAuthentication.Create().WithHeader("X-Token").Keys("123"))
                                      .Add(BasicAuthentication.Create().Add("bob", "secret"));

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        Assert.AreEqual(2, doc.Components?.SecuritySchemes?.Count);

        var op = GetOperations(doc).Single();

        Assert.AreEqual("apiKey | basic", GetSecurity(op.Security));

        Assert.AreEqual("The request could not be authenticated with any of the supported methods", op.Responses?["401"].Description);
        Assert.IsTrue(op.Responses?["401"].Headers?.ContainsKey("WWW-Authenticate"));
        Assert.IsTrue(op.Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNestedConcernsAreAllRequired(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Authentication(ApiKeyAuthentication.Create().Keys("123"))
                        .Add(BearerAuthentication.Create());

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("apiKey+bearer", GetSecurity(op.Security));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomConcernWithAlternativesAndRequirements(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Add(new DocumentingConcernBuilder(d => d.RequireAuthentication(SecurityScheme.Basic(), SecurityScheme.Bearer())
                                                                 .RequireAuthentication(SecurityScheme.ApiKey("X-Tenant"))));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("apiKey+basic | apiKey+bearer", GetSecurity(op.Security));
    }

    [TestMethod]
    public void TestRequirementNeedsScheme()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new OperationDocumentation(RequestMethod.Get).RequireAuthentication());
    }

    #endregion

    #region Scheme registration

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDifferentSchemesWithSameName(ServerEngine engine)
    {
        var api = Layout.Create()
                        .Add("a", Inline.Create().Get(() => 42).Authentication(ApiKeyAuthentication.Create().WithHeader("X-A").Keys("1")))
                        .Add("b", Inline.Create().Get(() => 42).Authentication(ApiKeyAuthentication.Create().WithHeader("X-B").Keys("1")));

        var doc = await GetDocumentAsync(api, engine);

        var schemes = doc.Components?.SecuritySchemes;

        Assert.AreEqual(2, schemes?.Count);
        Assert.AreEqual("X-A", schemes?["apiKey"].Name);
        Assert.AreEqual("X-B", schemes?["apiKey2"].Name);

        Assert.AreEqual("apiKey", GetSecurity(doc.Paths["/a/"].Operations?[HttpMethod.Get].Security));
        Assert.AreEqual("apiKey2", GetSecurity(doc.Paths["/b/"].Operations?[HttpMethod.Get].Security));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEqualSchemesAreShared(ServerEngine engine)
    {
        var auth = ApiKeyAuthentication.Create().Keys("1");

        var api = Layout.Create()
                        .Add("a", Inline.Create().Get(() => 42).Authentication(auth))
                        .Add("b", Inline.Create().Get(() => 42).Authentication(auth));

        var doc = await GetDocumentAsync(api, engine);

        Assert.AreEqual(1, doc.Components?.SecuritySchemes?.Count);

        Assert.AreEqual("apiKey", GetSecurity(doc.Paths["/a/"].Operations?[HttpMethod.Get].Security));
        Assert.AreEqual("apiKey", GetSecurity(doc.Paths["/b/"].Operations?[HttpMethod.Get].Security));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnsecuredContentHasNoSecurity(ServerEngine engine)
    {
        var api = Layout.Create()
                        .Add("secured", Inline.Create().Get(() => 42).Authentication(ApiKeyAuthentication.Create().Keys("1")))
                        .Add("public", Inline.Create().Get(() => 42));

        var doc = await GetDocumentAsync(api, engine);

        var op = doc.Paths["/public/"].Operations?[HttpMethod.Get];

        Assert.IsNull(op?.Security);
        Assert.IsFalse(op?.Responses?.ContainsKey("401"));
    }

    #endregion

    #region Precedence

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOperationOverridesResponse(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.Unauthorized, "Send your key")] () => 42)
                        .Authentication(ApiKeyAuthentication.Create().Keys("1"));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("Send your key", op.Responses?["401"].Description);
    }

    #endregion

    #region Roles

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequiredRole(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/admin", [RequireRole("ADMIN")] () => 42)
                        .Get("/user", () => 42);

        var doc = await GetDocumentAsync(api, engine);

        var admin = doc.Paths["/admin"].Operations?[HttpMethod.Get];

        Assert.IsTrue(admin?.Responses?.ContainsKey("401"));
        AssertX.Contains("ADMIN", admin?.Responses?["403"].Description);

        Assert.IsFalse(doc.Paths["/user"].Operations?[HttpMethod.Get].Responses?.ContainsKey("403"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequiredRoles(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([RequireRole("ADMIN", "AUDITOR")] () => 42);

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        AssertX.Contains("ADMIN, AUDITOR", op.Responses?["403"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRoleOverridesConcern(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([RequireRole("ADMIN")] () => 42)
                        .Authentication(ApiKeyAuthentication.Create().Keys("1"));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        AssertX.Contains("ADMIN", op.Responses?["403"].Description);
        Assert.AreEqual("apiKey", GetSecurity(op.Security));
    }

    #endregion

    #region Spec-wide security

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSpecWideSecurity(ServerEngine engine)
    {
        var description = ApiDescription.Create()
                                        .Security(SecurityScheme.Bearer(description: "Issued by the gateway"));

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42), engine, description);

        Assert.AreEqual("bearer", GetSecurity(doc.Security));
        Assert.AreEqual("Issued by the gateway", doc.Components?.SecuritySchemes?["bearer"].Description);

        Assert.IsNull(GetOperations(doc).Single().Security);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSpecWideSecurityIsCombinedWithConcerns(ServerEngine engine)
    {
        var description = ApiDescription.Create()
                                        .Security(SecurityScheme.Bearer());

        var api = Layout.Create()
                        .Add("secured", Inline.Create().Get(() => 42).Authentication(ApiKeyAuthentication.Create().Keys("1")))
                        .Add("public", Inline.Create().Get(() => 42));

        var doc = await GetDocumentAsync(api, engine, description);

        Assert.AreEqual("bearer", GetSecurity(doc.Security));

        Assert.AreEqual("apiKey+bearer", GetSecurity(doc.Paths["/secured/"].Operations?[HttpMethod.Get].Security));
        Assert.IsNull(doc.Paths["/public/"].Operations?[HttpMethod.Get].Security);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSpecWideAlternatives(ServerEngine engine)
    {
        var description = ApiDescription.Create()
                                        .Security(SecurityScheme.Bearer(), SecurityScheme.ApiKey("X-Gateway-Key"));

        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42), engine, description);

        Assert.AreEqual("apiKey | bearer", GetSecurity(doc.Security));
    }

    [TestMethod]
    public void TestSpecWideSecurityNeedsScheme()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ApiDescription.Create().Security());
    }

    #endregion

    #region Helpers

    private static async Task<OpenApiDocument> GetDocumentAsync<T>(T api, ServerEngine engine, OpenApiConcernBuilder? description = null) where T : IHandlerBuilder<T>
        => (await api.Add(description ?? ApiDescription.Create()).GetOpenApiAsync(engine)).Document!;

    private static async Task<IOpenApiSecurityScheme> GetSchemeAsync(IConcernBuilder auth, ServerEngine engine)
    {
        var doc = await GetDocumentAsync(Inline.Create().Get(() => 42).Add(auth), engine);

        return doc.Components!.SecuritySchemes!.Values.Single();
    }

    private static IEnumerable<OpenApiOperation> GetOperations(OpenApiDocument doc)
        => doc.Paths.Values.SelectMany(p => p.Operations?.Values.AsEnumerable() ?? []);

    /// <summary>
    /// Formats the given requirements as "a+b | c", alternatives and schemes in alphabetical order.
    /// </summary>
    private static string? GetSecurity(IList<OpenApiSecurityRequirement>? security)
    {
        if (security == null)
        {
            return null;
        }

        var alternatives = security.Select(r => string.Join('+', r.Keys.Select(k => k.Reference.Id).Order()))
                                   .Order();

        return string.Join(" | ", alternatives);
    }

    private sealed class DocumentingConcernBuilder(Action<OperationDocumentation> documentation) : IConcernBuilder
    {

        public IConcern Build(IHandler content) => new DocumentingConcern(content, documentation);

    }

    private sealed class DocumentingConcern(IHandler content, Action<OperationDocumentation> documentation) : IDocumentedConcern
    {

        public IHandler Content => content;

        public void AddDocumentation(OperationDocumentation operation) => documentation(operation);

        public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => content.HandleAsync(request);

    }

    #endregion

}
