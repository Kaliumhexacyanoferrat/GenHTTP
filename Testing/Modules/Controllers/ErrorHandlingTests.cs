using System;
using System.Net;

using Xunit;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    public class ErrorHandlingTests
    {

        #region Supporting data structures

        public class TestController
        {

            public int SimpleType() => 42;

        }

        public class ControllerWithNullablePath
        {

            public int Test([FromPath] int? id) => 42;

        }

        public class ComplexPath { }

        public class ControllerWithComplexPath
        {

            public int Test([FromPath] ComplexPath value) => 42;

        }

        #endregion

        #region Tests

        [Fact]
        public void TestMustNotReturnSimpleType()
        {
            using var response = Run("/c/simple-type/");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void TestNoNullablePathArguments()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var controller = Controller.From<ControllerWithNullablePath>();
                using var _ = TestRunner.Run(controller);
            });
        }

        [Fact]
        public void TestNoComplexPathArguments()
        {
            Assert.Throws<InvalidOperationException>(() =>
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
