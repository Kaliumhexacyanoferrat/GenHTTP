using System.Net;

using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class BasicAuthenticationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoUser(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create());

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidUser(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                     .Add("user", "password"));

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await GetResponse(runner, "user", "password");

        Assert.AreEqual("user", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalidPassword(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                     .Add("user", "password"));

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await GetResponse(runner, "user", "p");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvalidUser(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create());

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await GetResponse(runner, "u", "password");

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomUser(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create((_, _) => new ValueTask<IUser?>(new BasicAuthenticationUser("my"))));

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await GetResponse(runner, "_", "_");

        Assert.AreEqual("my", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoCustomUser(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create((_, _) => new ValueTask<IUser?>()));

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        using var response = await GetResponse(runner, "_", "_");

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOtherAuthenticationIsNotAccepted(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create());

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Authorization", "Bearer 123");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoValidBase64(TestEngine engine)
    {
        var content = GetContent().Authentication(BasicAuthentication.Create());

        await using var runner = await TestHost.RunAsync(content, engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Authorization", "Basic 123");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpResponseMessage> GetResponse(TestHost runner, string user, string password)
    {
        using var client = TestHost.GetClient(creds: new NetworkCredential(user, password));

        return await runner.GetResponseAsync(client: client);
    }

    private static LayoutBuilder GetContent() => Layout.Create()
                                                       .Index(new UserReturningHandler().Wrap());

}
