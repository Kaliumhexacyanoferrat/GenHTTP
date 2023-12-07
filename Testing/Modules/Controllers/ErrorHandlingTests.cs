using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class ErrorHandlingTests
    {

        #region Supporting data structures

        public sealed class TestController
        {

            public int SimpleType() => 42;

        }

        public sealed class ControllerWithNullablePath
        {

            public int Test([FromPath] int? id) => 42;

        }

        public sealed class ComplexPath { }

        public sealed class ControllerWithComplexPath
        {

            public int Test([FromPath] ComplexPath value) => 42;

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task TestMustNotReturnSimpleType()
        {
            using var response = await Run("/c/simple-type/");

            await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public void TestNoNullablePathArguments()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var controller = Controller.From<ControllerWithNullablePath>();
                using var _ = TestRunner.Run(controller);
            });
        }

        [TestMethod]
        public void TestNoComplexPathArguments()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var controller = Controller.From<ControllerWithComplexPath>();
                using var _ = TestRunner.Run(controller);
            });
        }

        #endregion

        #region Helpers

        private async Task<HttpResponseMessage> Run(string uri)
        {
            var layout = Layout.Create()
                               .AddController<TestController>("c");

            using var runner = TestRunner.Run(layout);

            return await runner.GetResponse(uri);
        }

        #endregion

    }

}
