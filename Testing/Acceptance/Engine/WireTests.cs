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

    /// <summary>
    /// A client spells the Connection header in whatever case it likes - browsers and curl both
    /// send "keep-alive" - so the value has to be compared case-insensitively. Two requests are
    /// pipelined onto one connection, the first asking to keep it alive in lowercase and the second
    /// to close it: honouring the first is exactly what lets the second be answered at all, so a
    /// case-sensitive comparison closes after the first and only one response comes back.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestKeepAliveIsCaseInsensitive(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Layout.Create(), engine: engine);

        var result = await SendAsync(host, w =>
        {
            w.Write($"GET / HTTP/1.1{NL}");
            w.Write($"Host: 127.0.0.1{NL}");
            w.Write($"Connection: keep-alive{NL}");
            w.Write($"{NL}");

            w.Write($"GET / HTTP/1.1{NL}");
            w.Write($"Host: 127.0.0.1{NL}");
            w.Write($"Connection: close{NL}");
            w.Write($"{NL}");
        });

        var responses = result.Split("HTTP/1.1 ").Length - 1;

        Assert.AreEqual(2, responses, "The lowercase keep-alive was not honoured, so the connection closed before the second request could be answered.");
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
