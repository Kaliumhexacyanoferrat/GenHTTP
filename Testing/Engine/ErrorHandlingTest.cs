using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ErrorHandlingTest
    {

        [TestMethod]
        public async Task TestGenericError()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                throw new NotImplementedException();
            });

            using var runner = TestRunner.Run(handler);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
        }

    }

}
