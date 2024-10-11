using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class BasicAuthenticationTests
{

    [TestMethod]
    public async Task TestNoUser()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestHost.Run(content);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

    [TestMethod]
    public async Task TestValidUser()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestHost.Run(content);

            using var response = await GetResponse(runner, "user", "password");

            Assert.AreEqual("user", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestInvalidPassword()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestHost.Run(content);

            using var response = await GetResponse(runner, "user", "p");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

    [TestMethod]
    public async Task TestInvalidUser()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestHost.Run(content);

            using var response = await GetResponse(runner, "u", "password");

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

    [TestMethod]
    public async Task TestCustomUser()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>(new BasicAuthenticationUser("my"))));

            using var runner = TestHost.Run(content);

            using var response = await GetResponse(runner, "_", "_");

            Assert.AreEqual("my", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestNoCustomUser()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>()));

            using var runner = TestHost.Run(content);

            using var response = await GetResponse(runner, "_", "_");

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

    [TestMethod]
    public async Task TestOtherAuthenticationIsNotAccepted()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestHost.Run(content);

            var request = runner.GetRequest();
            request.Headers.Add("Authorization", "Bearer 123");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

    [TestMethod]
    public async Task TestNoValidBase64()
    {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestHost.Run(content);

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

    private static LayoutBuilder GetContent()
    {
            return Layout.Create()
                         .Index(new UserReturningHandlerBuilder());
        }

}
