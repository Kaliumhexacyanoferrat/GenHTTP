using System;
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

            runner.Builder.Console().Build();

            using var _ = runner.GetResponse();
        }

        /// <summary>
        /// As a developer, I want to add custom companions to get notified by server actions.
        /// </summary>
        [Fact]
        public void TestCustom()
        {
            using var runner = new TestRunner();

            var companion = new CustomCompanion();

            runner.Builder.Companion(companion).Build();

            using var _ = runner.GetResponse();

            Assert.True(companion.Called);
        }

    }

}
