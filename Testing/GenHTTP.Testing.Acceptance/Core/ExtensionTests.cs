using System;
using System.Net;

using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules;

using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ExtensionTests
    {

        public class MyExtension : IServerExtension
        {

            public IContentProvider? Intercept(IRequest request) => null;

            public void Intercept(IRequest request, IResponse response)
            {
                if (request.Path == "/throw")
                {
                    throw new InvalidOperationException("Don't throw!");
                }

                response["X-Powered-By"] = "My Extension";
            }

        }

        public class MyExtensionBuilder : IServerExtensionBuilder
        {

            public IServerExtension Build()
            {
                return new MyExtension();
            }

        }

        /// <summary>
        /// As a developer, I can add additional extensions to intercept requests.
        /// </summary>
        [Fact]
        public void TestCustomExtension()
        {
            using var runner = new TestRunner();

            runner.Host.Extension(new MyExtensionBuilder()).Start();

            using var response = runner.GetResponse();

            Assert.Equal("My Extension", response.GetResponseHeader("X-Powered-By"));
        }

        /// <summary>
        /// As a developer, I can throw exceptions in extensions without taking the server down.
        /// </summary>
        [Fact]
        public void TestCustomExtensionThrowing()
        {
            using var runner = new TestRunner();

            runner.Host.Extension(new MyExtensionBuilder()).Start();

            for (var i = 0; i < 2; i++)
            {
                using var response = runner.GetResponse("/throw");
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

    }

}
