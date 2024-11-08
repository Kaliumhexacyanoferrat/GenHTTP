using System.Net;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public class UserInjectionTests
{

    #region Helpers

    private static async Task<TestHost> GetRunnerAsync(TestEngine engine)
    {
        var auth = BasicAuthentication.Create()
                                      .Add("abc", "def");

        var injection = Injection.Empty()
                                 .Add(new UserInjector<BasicAuthenticationUser>());

        var content = Inline.Create()
                            .Get((BasicAuthenticationUser user) => user.DisplayName)
                            .Injectors(injection)
                            .Authentication(auth);

        return await TestHost.RunAsync(content, engine: engine);
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUserInjected(TestEngine engine)
    {
        await using var runner = await GetRunnerAsync(engine);

        using var client = TestHost.GetClient(creds: new NetworkCredential("abc", "def"));

        using var response = await runner.GetResponseAsync(client: client);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("abc", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoUser(TestEngine engine)
    {
        await using var runner = await GetRunnerAsync(engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    #endregion

}
