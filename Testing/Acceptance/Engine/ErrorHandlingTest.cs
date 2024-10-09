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

            using var runner = TestHost.Run(handler.Wrap());

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public async Task TestEscaping()
        {
            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                throw new Exception("Nah <>");
            });


            using var runner = TestHost.Run(handler.Wrap());

            using var response = await runner.GetResponseAsync();

            AssertX.DoesNotContain("<>", await response.GetContentAsync());
        }

    }

}
