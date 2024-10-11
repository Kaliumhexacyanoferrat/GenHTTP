using System.Net.Sockets;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class WireTests
{
    private const string NL = "\r\n";

    #region Tests

    [TestMethod]
    public async Task TestLowerCaseRequests()
    {
        var app = Inline.Create().Get((IRequest r) => r.Headers["x-my-header"]);

        using var host = TestHost.Run(app);

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

        using var host = TestHost.Run(app);

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
        await TestAsync("GET / HTTP/1.0\r\nContent-Length: ABC\r\n", "Content-Length header is expected to be a numeric value");
    }

    [TestMethod]
    public async Task TestNoKeepAliveForHttp10()
    {
        using var host = TestHost.Run(Layout.Create());

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
        using var host = TestHost.Run(Layout.Create());

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
        using var host = TestHost.Run(Layout.Create());

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
        using var host = TestHost.Run(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write("This is no HTTP request but text");
        });

        AssertX.Contains("400 Bad Request", result);
        AssertX.Contains("HTTP protocol version expected", result);
    }

    #endregion

    #region Helpers

    private static async ValueTask TestAsync(string request, string assertion)
    {
        using var host = TestHost.Run(Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write(request);
            w.Write(NL);
        });

        AssertX.Contains(assertion, result);
    }

    private static async ValueTask<string> SendAsync(TestHost host, Action<StreamWriter> sender)
    {
        using var client = new TcpClient("127.0.0.1", host.Port)
        {
            ReceiveTimeout = 1000
        };

        var stream = client.GetStream();

        await using var writer = new StreamWriter(stream, leaveOpen: true);

        sender(writer);

        await writer.FlushAsync();

        using var reader = new StreamReader(stream, leaveOpen: true);

        return await reader.ReadToEndAsync();
    }

    #endregion

}
