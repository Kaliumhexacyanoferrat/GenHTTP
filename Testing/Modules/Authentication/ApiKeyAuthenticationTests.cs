using System;
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
        public void TestNoKey()
        {
            using var runner = GetRunnerWithKeys("123");

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            using var runner = GetRunnerWithKeys("123");

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "124");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public void TestValidKey()
        {
            using var runner = GetRunnerWithKeys("123");

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "123");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestValidKeyWithWhitespace()
        {
            using var runner = GetRunnerWithKeys("123");

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", " 123 ");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestValidKeyFromQuery()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .WithQueryParameter("key")
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            using var response = runner.GetResponse("/?key=123");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestValidKeyFromHeader()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .WithHeader("key")
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.Headers.Add("key", " 123 ");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestCustomExtractor()
        {
            var auth = ApiKeyAuthentication.Create()
                                           .Extractor((r) => r.UserAgent)
                                           .Keys("123");

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.UserAgent = "123";

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void TestCustomAuthenticator()
        {
            static ValueTask<IUser?> authenticator(IRequest r, string k) => (k.Length == 5) ? new ValueTask<IUser?>(new ApiKeyUser(k)) : new ValueTask<IUser?>();

            var auth = ApiKeyAuthentication.Create()
                                           .Authenticator(authenticator);

            using var runner = GetRunnerWithAuth(auth);

            var request = runner.GetRequest();
            request.Headers.Add("X-API-Key", "12345");

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private static TestRunner GetRunnerWithKeys(params string[] keys)
        {
            var auth = ApiKeyAuthentication.Create()
                                           .Keys(keys);

            return GetRunnerWithAuth(auth);
        }

        private static TestRunner GetRunnerWithAuth(ApiKeyConcernBuilder auth)
        {
            var content = GetContent().Authentication(auth);

            return TestRunner.Run(content);
        }

        private static LayoutBuilder GetContent() => Layout.Create().Fallback(Content.From(Resource.FromString("Hello World!")));

    }

}
