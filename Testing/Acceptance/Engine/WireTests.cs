using System.Net.Sockets;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class WireTests : WireTest
{

    #region Tests

    [TestMethod]
    public async Task TestLowerCaseRequests()
    {
        var app = Inline.Create().Get((IRequest r) => r.Headers["x-my-header"]);

        await using var host = await TestHost.RunAsync(app);

        var result = await SendAsync(host, w =>
        {
            w.Write($"get / http/1.0{NL}");
            w.Write($"host: 127.0.0.1{NL}");
            w.Write($"x-my-header: abc{NL}");
            w.Write($"{NL}");
        });

        AssertX.Contains("abc", result);
    }

    [TestMethod]
    public async Task TestWhitespaceRequest()
    {
        var app = Inline.Create().Get("/some-path/", (IRequest r) => r.Headers["X-My-Header"]);

        await using var host = await TestHost.RunAsync(app);

        var result = await SendAsync(host, w =>
        {
            w.Write($" GET  /some-path/  HTTP/1.0{NL}");
            w.Write($" Host : 127.0.0.1 {NL}");
            w.Write($"    X-My-Header: abc     {NL}");
            w.Write($"{NL}");
        });

        AssertX.Contains("abc", result);
    }

    [TestMethod]
    public async Task TestNoHost()
    {
        await TestAsync("GET / HTTP/1.0\r\n", "Host");
    }

    [TestMethod]
    public async Task TestUnsupportedProtocolVersion()
    {
        await TestAsync("GET / HTTP/2.0\r\n", "Unexpected protocol version");
    }

    [TestMethod]
    public async Task TestUnexpectedProtocol()
    {
        await TestAsync("GET / GENHTTP/1.0\r\n", "HTTP protocol version expected");
    }

    [TestMethod]
    public async Task TestContentLengthNotNumeric()
    {
        await TestAsync("GET / HTTP/1.0\r\nContent-Length: ABC\r\n", "Unable to parse the given 'Content-Length' header");
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

    [TestMethod]
    public async Task TestNonHttp()
    {
        await using var host = await TestHost.RunAsync(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write("{abc}");
        });

        AssertX.Contains("400 Bad Request", result);
        AssertX.Contains("Unable to read HTTP verb from request line", result);
    }

    [TestMethod]
    public async Task TestNonHttpButText()
    {
        await using var host = await TestHost.RunAsync(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write("This is no HTTP request but text");
        });

        AssertX.Contains("400 Bad Request", result);
        AssertX.Contains("HTTP protocol version expected", result);
    }

    #endregion

}
