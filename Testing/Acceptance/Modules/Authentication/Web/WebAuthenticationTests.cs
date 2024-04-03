﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication.Web
{

    [TestClass]
    public sealed class WebAuthenticationTests
    {

        #region Tests

        [TestMethod]
        public async Task TestHappyPath()
        {
            using var host = GetHost();

            await Post(host, "/setup/", "u", "p");

            await Post(host, "/login/", "u", "p");

            var content = await host.GetResponseAsync("/content/");

            await content.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("Hello World", await content.GetContentAsync());
        }

        [TestMethod]
        public async Task TestSetupEnforced()
        {
            using var host = GetHost();

            using var response = await host.GetResponseAsync("/content");

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);

            Assert.IsTrue(response.GetHeader("Location")?.EndsWith("/setup/"));
        }

        [TestMethod]
        public async Task TestLoginRequired()
        {
            var integration = new TestIntegration().AddUser("a", "b");

            using var host = GetHost(integration);

            using var response = await host.GetResponseAsync("/content");

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);

            Assert.IsTrue(response.GetHeader("Location")?.EndsWith("/login/"));
        }

        [TestMethod]
        public async Task TestSetupFormRenders()
        {
            using var host = GetHost();

            using var response = await host.GetResponseAsync("/setup/");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            var content = await response.GetContentAsync();

            AssertX.Contains("id=\"user\"", content);
        }

        [TestMethod]
        public async Task TestLoginFormRenders()
        {
            var integration = new TestIntegration().AddUser("a", "b");

            using var host = GetHost(integration);

            using var response = await host.GetResponseAsync("/login/");

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);

            var content = await response.GetContentAsync();

            AssertX.Contains("id=\"user\"", content);
        }

        [TestMethod]
        public async Task TestSuccessfulSetupRedirectsToLogin()
        {
            using var host = GetHost();

            using var response = await Post(host, "/setup/", "u", "p");

            await response.AssertStatusAsync(HttpStatusCode.RedirectMethod);

            var content = await response.GetContentAsync();

            Assert.IsTrue(response.GetHeader("Location")?.EndsWith("/login/"));
        }

        [TestMethod]
        public async Task TestCannotSetupWithBadParams()
        {
            using var host = GetHost();

            using var response = await Post(host, "/setup/", "", "");

            await response.AssertStatusAsync(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task TestCannotLoginWithBadParams()
        {
            var integration = new TestIntegration().AddUser("a", "b");

            using var host = GetHost(integration);

            using var response = await Post(host, "/login/", "", "");

            await response.AssertStatusAsync(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task TestCannotLoginWithWrongPassword()
        {
            var integration = new TestIntegration().AddUser("a", "b");

            using var host = GetHost(integration);

            using var response = await Post(host, "/login/", "a", "c");

            await response.AssertStatusAsync(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Test setup

        private async Task<HttpResponseMessage> Post(TestHost host, string route, string username, string password)
        {
            var request = host.GetRequest(route, HttpMethod.Post);

            var args = new List<KeyValuePair<string, string>>() 
            {
                new("user", username), new("password", password)
            };

            request.Content = new FormUrlEncodedContent(args);

            return await host.GetResponseAsync(request);
        }

        private TestHost GetHost(TestIntegration? integration = null)
        {
            var auth = WebAuthentication.Simple(integration ?? new());

            var content = Layout.Create()
                                .Add("content", Content.From(Resource.FromString("Hello World")))
                                .Add(auth);

            return TestHost.Run(content);
        }

        private class TestIntegration : ISimpleWebAuthIntegration
        {
            private readonly List<MyUser> _Users = new();

            private readonly Dictionary<string, MyUser> _Sessions = new();

            public TestIntegration AddUser(string username, string password) 
            { 
                _Users.Add(new(username, password));
                return this;
            }

            public ValueTask<bool> CheckSetupRequired(IRequest request) => new(_Users.Count == 0);

            public ValueTask PerformSetup(IRequest request, string username, string password)
            {
                _Users.Add(new(username, password));
                return new();
            }

            public ValueTask<IUser?> PerformLogin(IRequest request, string username, string password)
            {
                return new(_Users.FirstOrDefault(e => e.Name == username && e.Password == password));
            }

            public ValueTask<string> StartSessionAsync(IRequest request, IUser user)
            {
                var token = Guid.NewGuid().ToString();
                _Sessions.Add(token, (MyUser)user);
                return new(token);
            }

            public ValueTask<IUser?> VerifyTokenAsync(string sessionToken)
            {
                if (_Sessions.TryGetValue(sessionToken, out var user))
                {
                    return new(user);
                }

                return new();
            }

        }

        public class MyUser : IUser
        {

            public string Name { get; }

            public string Password { get; }

            public string DisplayName => Name;

            public MyUser(string name, string password) { Name = name; Password = password; }

        }

        #endregion

    }

}
