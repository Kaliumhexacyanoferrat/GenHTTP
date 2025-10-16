using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Multi;
using GenHTTP.Modules.Functional;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class MultiAuthenticationTests
{
    #region Supporting data structures

    private const string ValidBearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private class MockHandler : IHandler
    {
        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            throw new NotImplementedException();
        }

        public ValueTask PrepareAsync()
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    [TestMethod]
    public void TestConcernFullBuild()
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create())
            .Add(ApiKeyAuthentication.Create().Authenticator((_, _) => ValueTask.FromResult<IUser?>(null)))
            .Add(BasicAuthentication.Create())
            .Add(BasicAuthentication.Create((_, _) => ValueTask.FromResult<IUser?>(null)))
            .Add(ClientCertificateAuthentication.Create())
            ;

        var concern = builder.Build(new MockHandler());

        Assert.IsInstanceOfType<MultiAuthenticationConcern>(concern);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSingleBearerPass(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create().AllowExpired())
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidBearerToken));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSingleBearerFail(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create().AllowExpired())
            ;

        using var response = await Execute(builder, engine);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSingleBasicPass(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BasicAuthentication.Create().Add("user", "pass"))
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass"))));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSingleBasicFail(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BasicAuthentication.Create().Add("user", "pass"))
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("user:invalidpass"))));

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        AssertBasicChallenge(response);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCombinedFirstPass(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create().AllowExpired())
            .Add(BasicAuthentication.Create().Add("user", "pass"))
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidBearerToken));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCombinedSecondPass(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create().AllowExpired())
            .Add(BasicAuthentication.Create().Add("user", "pass"))
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass"))));

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCombinedFail(TestEngine engine)
    {
        var builder = MultiAuthentication
            .Create()
            .Add(BearerAuthentication.Create().AllowExpired())
            .Add(BasicAuthentication.Create().Add("user", "pass"))
            ;

        using var response = await Execute(builder, engine,
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("user:invalidpass"))));

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        AssertBasicChallenge(response);
    }

    [TestMethod]
    [MultiEngineTest]
    public void TestEmpty(TestEngine engine)
    {
        var builder = MultiAuthentication.Create();

        Assert.ThrowsExactly<BuilderMissingPropertyException>(() =>
        {
            using var response = Execute(builder, engine).GetAwaiter().GetResult();
        });
    }

    private static void AssertBasicChallenge(HttpResponseMessage response)
    {
        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault(x => x.Scheme == "Basic" && (x.Parameter?.StartsWith("realm") ?? false)));
    }

    private static async Task<HttpResponseMessage> Execute(MultiAuthenticationConcernBuilder builder, TestEngine engine, Action<HttpRequestMessage>? authAction = null)
    {
        var handler = Inline.Create()
                            .Get(() => "Secured")
                            .Add(builder);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        var request = host.GetRequest();

        authAction?.Invoke(request);

        return await host.GetResponseAsync(request);
    }
}
