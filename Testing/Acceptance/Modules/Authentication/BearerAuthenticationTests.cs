using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Functional;

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
