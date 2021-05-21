using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ErrorHandlingTest
    {

        [TestMethod]
        public void TestGenericError()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                throw new NotImplementedException();
            });

            using var runner = TestRunner.Run(handler);

            var request = runner.GetRequest();

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }

}
