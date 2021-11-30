﻿using System.Net;
using System.Threading.Tasks;

using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class HeaderTests
    {

        [TestMethod]
        public async Task TestServerHeaderCanBeSet()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Header("Server", "TFB")
                        .Build();
            });

            using var runner = TestRunner.Run(handler);

            using var response = await runner.GetResponse();

            Assert.AreEqual("TFB", response.GetHeader("Server"));
        }

        [TestMethod]
        public async Task TestReservedHeaderCannotBeSet()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Header("Date", "123")
                        .Build();
            });

            using var runner = TestRunner.Run(handler);

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }

}
