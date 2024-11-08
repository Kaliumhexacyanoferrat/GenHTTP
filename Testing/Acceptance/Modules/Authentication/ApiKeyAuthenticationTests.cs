using System.Net;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class ApiKeyAuthenticationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoKey(TestEngine engine)
    {
        await using var runner = await GetRunnerWithKeysAsync(engine, "123");

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalidKey(TestEngine engine)
    {
        await using var runner = await GetRunnerWithKeysAsync(engine, "123");

        var request = runner.GetRequest();
        request.Headers.Add("X-API-Key", "124");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidKey(TestEngine engine)
    {
        await using var runner = await GetRunnerWithKeysAsync(engine, "123");

        var request = runner.GetRequest();
        request.Headers.Add("X-API-Key", "123");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidKeyFromQuery(TestEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .WithQueryParameter("key")
                                       .Keys("123");

        await using var runner = await GetRunnerWithAuthAsync(auth, engine);

        using var response = await runner.GetResponseAsync("/?key=123");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidKeyFromHeader(TestEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .WithHeader("key")
                                       .Keys("123");

        await using var runner = await GetRunnerWithAuthAsync(auth, engine);

        var request = runner.GetRequest();
        request.Headers.Add("key", "123");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomExtractor(TestEngine engine)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Extractor(r => r.UserAgent)
                                       .Keys("123");

        await using var runner = await GetRunnerWithAuthAsync(auth, engine);

        var request = runner.GetRequest();
        request.Headers.Add("User-Agent", "123");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomAuthenticator(TestEngine engine)
    {
        static ValueTask<IUser?> Authenticator(IRequest r, string k) => k.Length == 5 ? new ValueTask<IUser?>(new ApiKeyUser(k)) : new ValueTask<IUser?>();

        var auth = ApiKeyAuthentication.Create()
                                       .Authenticator(Authenticator);

        await using var runner = await GetRunnerWithAuthAsync(auth, engine);

        var request = runner.GetRequest();
        request.Headers.Add("X-API-Key", "12345");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    private static async Task<TestHost> GetRunnerWithKeysAsync(TestEngine engine, params string[] keys)
    {
        var auth = ApiKeyAuthentication.Create()
                                       .Keys(keys);

        return await GetRunnerWithAuthAsync(auth, engine);
    }

    private static async Task<TestHost> GetRunnerWithAuthAsync(ApiKeyConcernBuilder auth, TestEngine engine)
    {
        var content = GetContent().Authentication(auth);

        return await TestHost.RunAsync(content, engine: engine);
    }

    private static LayoutBuilder GetContent() => Layout.Create().Add(Content.From(Resource.FromString("Hello World!")));
}
