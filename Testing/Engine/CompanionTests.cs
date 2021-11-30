﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class CompanionTests
    {

        private class CustomCompanion : IServerCompanion
        {

            public bool Called { get; private set; }

            public void OnRequestHandled(IRequest request, IResponse response)
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
        [TestMethod]
        public async Task TestConsole()
        {
            using var runner = new TestRunner();

            runner.Host.Console().Start();

            using var __ = await runner.GetResponse();
        }

        /// <summary>
        /// As a developer, I want to add custom companions to get notified by server actions.
        /// </summary>
        [TestMethod]
        public async Task TestCustom()
        {
            using var runner = new TestRunner();

            var companion = new CustomCompanion();

            runner.Host.Companion(companion).Start();

            using var __ = await runner.GetResponse();

            // the companion is called _after_ the response has been sent
            // bad hack, reconsider
            Thread.Sleep(50);

            Assert.IsTrue(companion.Called);
        }

    }

}
