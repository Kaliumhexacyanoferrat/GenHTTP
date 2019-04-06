using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class DeveloperModeTests
    {

        private class ThrowingProvider : IContentProvider
        {

            public IResponseBuilder Handle(IRequest request)
            {
                throw new InvalidOperationException("Nope!");
            }

        }

        /// <summary>
        /// As a developer of a web project, I would like to see exceptions rendered 
        /// in the browser, so that I can trace an error more quickly
        /// </summary>
        [Fact]
        public void TestExceptionsWithTrace()
        {
            using var runner = new TestRunner();

            var router = Layout.Create().Add("index", new ThrowingProvider(), true);

            using var _ = runner.Builder.Router(router).Development().Build();

            using var response = runner.GetResponse();

            Assert.Contains("Exception", response.GetContent());
        }

        /// <summary>
        /// As a devops member, I do not want an web application to leak internal
        /// implementation detail with exception messages
        /// </summary>
        [Fact]
        public void TestExceptionsWithNoTrace()
        {
            var router = Layout.Create().Add("index", new ThrowingProvider(), true);

            using var runner = TestRunner.Run(router);

            using var response = runner.GetResponse();

            Assert.DoesNotContain("Exception", response.GetContent());
        }

    }

}
