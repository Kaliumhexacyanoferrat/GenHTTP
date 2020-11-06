using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication
{

    public class BasicAuthenticationTests
    {

        #region Supporting data structures

        class UserReturningHandlerBuilder : IHandlerBuilder
        {

            public IHandler Build(IHandler parent) => new UserReturningHandler(parent);
        
        }

        class UserReturningHandler : IHandler
        {

            public IHandler Parent { get; }

            public UserReturningHandler(IHandler parent)
            {
                Parent = parent;
            }

            public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                var content = request.GetUser<IUser>()?.DisplayName ?? throw new ProviderException(ResponseStatus.BadRequest, "No user!");

                return request.Respond()
                              .Content(content)
                              .BuildTask();
            }

        }

        #endregion

        [Fact]
        public void TestNoUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void TestValidUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "user", "password");

            Assert.Equal("user", response.GetContent());
        }

        [Fact]
        public void TestInvalidPassword()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create()
                                                                         .Add("user", "password"));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "user", "p");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void TestInvalidUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "u", "password");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void TestCustomUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>(new BasicAuthenticationUser("my"))));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "_", "_");

            Assert.Equal("my", response.GetContent());
        }

        [Fact]
        public void TestNoCustomUser()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create((u, p) => new ValueTask<IUser?>()));

            using var runner = TestRunner.Run(content);

            using var response = GetResponse(runner, "_", "_");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void TestOtherAuthenticationIsNotAccepted()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            
            request.Headers.Add("Authorization", "Bearer 123");

            using var response = request.GetSafeResponse();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public void TestNoValidBase64()
        {
            var content = GetContent().Authentication(BasicAuthentication.Create());

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            
            request.Headers.Add("Authorization", "Basic 123");

            using var response = request.GetSafeResponse();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private HttpWebResponse GetResponse(TestRunner runner, string user, string password)
        {
            var request = runner.GetRequest();

            request.Credentials = new NetworkCredential(user, password);

            return request.GetSafeResponse();
        }

        private LayoutBuilder GetContent()
        {
            return Layout.Create()
                         .Index(new UserReturningHandlerBuilder());
        }

    }

}
