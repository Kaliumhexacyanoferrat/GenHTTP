using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Controllers;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class ErrorHandlingTests
{

    #region Supporting data structures

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
    public void TestNoNullablePathArguments()
    {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var controller = Controller.From<ControllerWithNullablePath>();
                using var _ = TestHost.Run(controller);
            });
        }

    [TestMethod]
    public void TestNoComplexPathArguments()
    {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var controller = Controller.From<ControllerWithComplexPath>();
                using var _ = TestHost.Run(controller);
            });
        }

    #endregion

}