﻿using System.Net;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication
{

    [TestClass]
    public sealed class BasicAuthenticationTests
    {

        [TestMethod]
        public void TestNoUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestValidUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "user", "password");

            Assert.AreEqual("user", response.GetContent());
        }

        [TestMethod]
        public void TestInvalidPassword()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "user", "p");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestInvalidUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "u", "password");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestCustomUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>(new BasicAuthenticationUser("my"))));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "_", "_");

            Assert.AreEqual("my", response.GetContent());
        }

        [TestMethod]
        public void TestNoCustomUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>()));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "_", "_");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestOtherAuthenticationIsNotAccepted()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            
            request.Headers.Add("Authorization", "Bearer 123");

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [TestMethod]
        public void TestNoValidBase64()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            
            request.Headers.Add("Authorization", "Basic 123");

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static HttpWebResponse GetResponse(TestRunner runner, string user, string password)
        {
            var request = runner.GetRequest();

            request.Credentials = new NetworkCredential(user, password);

            return request.GetSafeResponse();
        }

        private static LayoutBuilder GetContent()
        {
            return Layout.Create()
                         .Index(new UserReturningHandlerBuilder());
        }

    }

}
