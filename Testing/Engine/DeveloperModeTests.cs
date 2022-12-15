using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class DeveloperModeTests
    {

        private class ThrowingProvider : IHandler
        {

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                throw new InvalidOperationException("Nope!");
            }

        }

        /// <summary>
        /// As a developer of a web project, I would like to see exceptions rendered 
        /// in the browser, so that I can trace an error more quickly
        /// </summary>
        [TestMethod]
        public async Task TestExceptionsWithTrace()
        {
            using var runner = new TestRunner();

            var router = Layout.Create().Index(new ThrowingProvider().Wrap());

            runner.Host.Handler(router).Development().Start();

            using var response = await runner.GetResponse();

            Assert.IsTrue((await response.GetContent()).Contains("Exception"));
        }

        /// <summary>
        /// As a devops member, I do not want an web application to leak internal
        /// implementation detail with exception messages
        /// </summary>
        [TestMethod]
        public async Task TestExceptionsWithNoTrace()
        {
            var router = Layout.Create().Index(new ThrowingProvider().Wrap());

            using var runner = TestRunner.Run(router);

            using var response = await runner.GetResponse();

            Assert.IsFalse((await response.GetContent()).Contains("Exception"));
        }

    }

}
