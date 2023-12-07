using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class HostTests
    {

        [TestMethod]
        public async Task TestStart()
        {
            using var runner = new TestRunner();

            runner.Host.Start();

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestRestart()
        {
            using var runner = new TestRunner();

            runner.Host.Restart();

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    }

}
