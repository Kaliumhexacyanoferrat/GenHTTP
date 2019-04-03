using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class CompanionTests
    {

        private class CustomCompanion : IServerCompanion
        {

            public bool Called { get; private set; }

            public void OnRequestHandled(IRequest request, IResponse response, Exception? error)
            {
                Called = true;
            }

            public void OnServerError(ServerErrorScope scope, Exception error)
            {
                Called = true;
            }

        }

        /// <summary>
        /// As a developer, I want to configure the server to easily log to the console.
        /// </summary>
        [Fact]
        public void TestConsole()
        {
            using var runner = new TestRunner();

            using var _ = runner.Builder.Console().Build();

            using var __ = runner.GetResponse();
        }

        /// <summary>
        /// As a developer, I want to add custom companions to get notified by server actions.
        /// </summary>
        [Fact]
        public void TestCustom()
        {
            using var runner = new TestRunner();

            var companion = new CustomCompanion();

            using var _ = runner.Builder.Companion(companion).Build();

            using var __ = runner.GetResponse();

            // the companion is called _after_ the response has been sent
            // bad hack, reconsider
            Thread.Sleep(50);

            Assert.True(companion.Called);
        }

    }

}
