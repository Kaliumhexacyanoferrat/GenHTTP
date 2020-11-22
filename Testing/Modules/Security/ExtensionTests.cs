using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;

namespace GenHTTP.Testing.Acceptance.Modules.Security
{

    [TestClass]
    public sealed class ExtensionTests
    {

        [TestMethod]
        public void ServerCanBeHardened()
        {
            using var runner = new TestRunner();

            runner.Host.Harden()
                       .Start();
        }

    }

}
