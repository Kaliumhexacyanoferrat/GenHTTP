using System;
using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class BasicTests
{

    [TestMethod]
    public async Task TestBuilder()
    {
            using var runner = new TestHost(Layout.Create());

            runner.Host.RequestMemoryLimit(128)
                       .TransferBufferSize(128)
                       .RequestReadTimeout(TimeSpan.FromSeconds(2))
                       .Backlog(1);

            runner.Start();

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    [TestMethod]
    public async Task TestLegacyHttp()
    {
            using var runner = TestHost.Run(Layout.Create());

            using var client = TestHost.GetClient(protocolVersion: new Version(1, 0));

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    [TestMethod]
    public async Task TestConnectionClose()
    {
            using var runner = TestHost.Run(Layout.Create());

            var request = runner.GetRequest();
            request.Headers.Add("Connection", "close");

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
            Assert.IsTrue(response.Headers.Connection.Contains("Close"));
        }

    [TestMethod]
    public async Task TestEmptyQuery()
    {
            using var runner = TestHost.Run(Layout.Create());

            using var response = await runner.GetResponseAsync("/?");

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    [TestMethod]
    public async Task TestKeepalive()
    {
            using var runner = TestHost.Run(Layout.Create());

            using var response = await runner.GetResponseAsync();

            Assert.IsTrue(response.Headers.Connection.Contains("Keep-Alive"));
        }

}