using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class WireTests : WireTest
{

    #region Tests
    
    [TestMethod]
    public async Task TestNoHost()
    {
        await TestAsync("GET / HTTP/1.0\r\n", "Host");
    }

    [TestMethod]
    public async Task TestUnsupportedProtocolVersion()
    {
        await TestAsync("GET / HTTP/2.0\r\n", "Invalid HTTP version");
    }

    [TestMethod]
    public async Task TestUnexpectedProtocol()
    {
        await TestAsync("GET / GENHTTP/1.0\r\n", "Invalid HTTP version");
    }

    [TestMethod]
    public async Task TestNoKeepAliveForHttp10()
    {
        await using var host = await TestHost.RunAsync(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write($"GET / HTTP/1.0{NL}");
            w.Write($"Host: 127.0.0.1{NL}");
            w.Write($"{NL}");
        });

        AssertX.DoesNotContain("Keep-Alive", result);
    }

    [TestMethod]
    public async Task TestNoKeepAliveForConnectionClose()
    {
        await using var host = await TestHost.RunAsync(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write($"GET / HTTP/1.1{NL}");
            w.Write($"Host: 127.0.0.1{NL}");
            w.Write($"Connection: close{NL}");
            w.Write($"{NL}");
        });

        AssertX.DoesNotContain("Keep-Alive", result);
    }

    #endregion

}
