using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;
using System.Net;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ExtensionTests
    {

        public class MyExtension : IServerExtension
        {

            public void Intercept(IRequest request, IResponse response)
            {
                if (request.Path == "/throw")
                {
                    throw new InvalidOperationException("Don't throw!");
                }

                response["X-Powered-By"] = "My Extension";
            }

        }

        /// <summary>
        /// As a developer, I can add additional extensions to intercept requests.
        /// </summary>
        [Fact]
        public void TestCustomExtension()
        {
            using var runner = new TestRunner();

            using var _ = runner.Builder.Extension(new MyExtension()).Build();

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

            using var _ = runner.Builder.Extension(new MyExtension()).Build();

            for (var i = 0; i < 2; i++)
            {
                using var response = runner.GetResponse("/throw");
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

    }

}
