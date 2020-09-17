using System;
using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class DeveloperModeTests
    {

        private class ThrowingProvider : IHandler
        {

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
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

            var router = Layout.Create().Index(new ThrowingProvider().Wrap());

            runner.Host.Handler(router).Development().Start();

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
            var router = Layout.Create().Index(new ThrowingProvider().Wrap());

            using var runner = TestRunner.Run(router);

            using var response = runner.GetResponse();

            Assert.DoesNotContain("Exception", response.GetContent());
        }

    }

}
