using System.Net;
using System.Net.Sockets;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class BindingTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAnyIPv4NoDualStack(TestEngine engine)
    {
        await using var runner = await RunWith(IPAddress.Any, false, engine);

        Assert.IsTrue(await CanConnectAsync(IPAddress.Loopback, runner.Port));
        Assert.IsFalse(await CanConnectAsync(IPAddress.IPv6Loopback, runner.Port));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAnyIPv4WithDualStack(TestEngine engine)
    {
        await using var runner = await RunWith(IPAddress.Any, true, engine);

        Assert.IsTrue(await CanConnectAsync(IPAddress.Loopback, runner.Port));
        Assert.IsTrue(await CanConnectAsync(IPAddress.IPv6Loopback, runner.Port));
    }

    // Kestrel-only: Kestrel's socket transport binds IPv6Any without forcing IPV6_V6ONLY,
    // so a request against the IPv4 loopback still connects even with dualStack = false.
    [TestMethod]
    public async Task TestAnyIPv6NoDualStack()
    {
        await using var runner = await RunWith(IPAddress.IPv6Any, false, TestEngine.Internal);

        Assert.IsFalse(await CanConnectAsync(IPAddress.Loopback, runner.Port));
        Assert.IsTrue(await CanConnectAsync(IPAddress.IPv6Loopback, runner.Port));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAnyIPv6WithDualStack(TestEngine engine)
    {
        await using var runner = await RunWith(IPAddress.IPv6Any, true, engine);

        Assert.IsTrue(await CanConnectAsync(IPAddress.Loopback, runner.Port));
        Assert.IsTrue(await CanConnectAsync(IPAddress.IPv6Loopback, runner.Port));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDualStackByDefault(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create());

        Assert.IsTrue(await CanConnectAsync(IPAddress.Loopback, runner.Port));
        Assert.IsTrue(await CanConnectAsync(IPAddress.IPv6Loopback, runner.Port));
    }

    #endregion

    #region Helpers

    private static async Task<TestHost> RunWith(IPAddress? ip, bool dualStack, TestEngine engine)
    {
        var runner = new TestHost(Layout.Create().Build(), engine: engine);

        runner.Host.Bind(ip, (ushort)runner.Port, HttpProtocols.All, dualStack);

        await runner.StartAsync();

        return runner;
    }

    private static async Task<bool> CanConnectAsync(IPAddress address, int port, int timeoutMs = 500)
    {
        try
        {
            using var client = new TcpClient(address.AddressFamily);

            var connectTask = client.ConnectAsync(address, port);

            if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) == connectTask)
            {
                return client.Connected;
            }
        }
        catch
        {
            // nop
        }

        return false;
    }

    #endregion

}
