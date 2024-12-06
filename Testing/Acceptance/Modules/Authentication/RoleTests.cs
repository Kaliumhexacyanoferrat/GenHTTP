using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public class RoleTests
{

    #region Supporting data structures

    public class UserSettingConcernBuilder(IUser? user) : IConcernBuilder
    {

        public IConcern Build(IHandler content) => new UserSettingConcern(content, user);

    }

    public class UserSettingConcern : IConcern
    {

        public IHandler Content { get; }

        public IUser? User { get; }

        public UserSettingConcern(IHandler content, IUser? user)
        {
            Content = content;
            User = user;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (User != null)
            {
                request.SetUser(User);
            }

            return Content.HandleAsync(request);
        }

    }

    public class RoleUser(string[]? roles) : IUser
    {

        public string DisplayName => "Role User";

        public string[]? Roles => roles;

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoUser(TestEngine engine)
    {
        using var response = await RunAsync(null, engine);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoRoles(TestEngine engine)
    {
        using var response = await RunAsync(new RoleUser(null), engine);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInsufficientRoles(TestEngine engine)
    {
        using var response = await RunAsync(new RoleUser(["ADMIN"]), engine);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);

        AssertX.Contains("SUPER_ADMIN", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSufficientRoles(TestEngine engine)
    {
        using var response = await RunAsync(new RoleUser(["ADMIN", "SUPER_ADMIN"]), engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCasingDoesNotMatter(TestEngine engine)
    {
        using var response = await RunAsync(new RoleUser(["admin", "Super_Admin"]), engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }


    [TestMethod]
    [MultiEngineTest]
    public async Task TestOtherRolesDoNotMatter(TestEngine engine)
    {
        using var response = await RunAsync(new RoleUser(["ADMIN", "USER", "SUPER_ADMIN"]), engine);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    #endregion

    #region Helpers

    private static async Task<HttpResponseMessage> RunAsync(IUser? user, TestEngine engine)
    {
        var app = Inline.Create()
                        .Get([RequireRole("ADMIN", "SUPER_ADMIN")]() => 42)
                        .Add(new UserSettingConcernBuilder(user));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        return await host.GetResponseAsync();
    }

    #endregion

}
