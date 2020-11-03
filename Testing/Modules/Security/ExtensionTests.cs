using Xunit;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;

namespace GenHTTP.Testing.Acceptance.Modules.Security
{

    public class ExtensionTests
    {

        [Fact]
        public void ServerCanBeHardened()
        {
            using var runner = new TestRunner();

            runner.Host.Harden()
                       .Start();
        }

    }

}
