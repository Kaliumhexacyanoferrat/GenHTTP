using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication
{

    [TestClass]
    public sealed class ApiKeyAuthenticationTests
    {

        [TestMethod]
        public async Task TestNoKey()
        {
            using var runner = GetRunnerWithKeys("123");

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TestInvalidKey()
        {
            using var runner = GetRunnerWithKeys("123");

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "124");

            using var response = await runner.GetResponseAsync(request);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task TestValidKey()
        {
            using var runner = GetRunnerWithKeys("123");

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "123");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task TestValidKeyFromQuery()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .WithQueryParameter("key")
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            using var response = await runner.GetResponseAsync("/?key=123");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task TestValidKeyFromHeader()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .WithHeader("key")
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.Headers.Add("key", "123");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task TestCustomExtractor()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .Extractor((r) => r.UserAgent)
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.Headers.Add("User-Agent", "123");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task TestCustomAuthenticator()
        {
            static ValueTask<IUser?> authenticator(IRequest r, string k) => (k.Length == 5) ? new ValueTask<IUser?>(new ApiKeyUser(k)) : new ValueTask<IUser?>();

            var auth = ApiKeyAuthentication.Create()
                                           .Authenticator(authenticator);

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "12345");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }

        private static TestHost GetRunnerWithKeys(params string[] keys)
        {
            var auth = ApiKeyAuthentication.Create()
                                           .Keys(keys);

            return GetRunnerWithAuth(auth);
        }

        private static TestHost GetRunnerWithAuth(ApiKeyConcernBuilder auth)
        {
            var content = GetContent().Authentication(auth);

            return TestHost.Run(content);
        }

        private static LayoutBuilder GetContent() => Layout.Create().Add(Content.From(Resource.FromString("Hello World!")));

    }

}
