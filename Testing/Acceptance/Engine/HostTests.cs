using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class HostTests
{

    [TestMethod]
    public async Task TestStart()
    {
            using var runner = new TestHost(Layout.Create());

            runner.Host.Start();

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    [TestMethod]
    public async Task TestRestart()
    {
            using var runner = new TestHost(Layout.Create());

            runner.Host.Restart();

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

}
