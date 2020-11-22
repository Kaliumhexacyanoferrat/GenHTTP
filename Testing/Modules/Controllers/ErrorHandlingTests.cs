using System;
using System.Net;

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
        public void TestMustNotReturnSimpleType()
        {
            using var response = Run("/c/simple-type/");

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
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

        private HttpWebResponse Run(string uri)
        {
            var layout = Layout.Create()
                               .AddController<TestController>("c");

            using var runner = TestRunner.Run(layout);

            return runner.GetResponse(uri);
        }

        #endregion

    }

}
