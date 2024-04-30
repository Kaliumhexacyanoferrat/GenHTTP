using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class WireTests
    {

        #region Tests

        [TestMethod]
        public async Task TestLowerCaseRequests()
        {
            var app = Inline.Create().Get((IRequest r) => r.Headers["x-my-header"]);

            using var host = TestHost.Run(app);

            var result = await SendAsync(host, (w) =>
            {
                w.WriteLine("get / http/1.0");
                w.WriteLine("host: 127.0.0.1");
                w.WriteLine("x-my-header: abc");
                w.WriteLine();
            });

            AssertX.Contains("abc", result);
        }

        [TestMethod]
        public async Task TestWhitespaceRequest()
        {
            var app = Inline.Create().Get("/some-path/", (IRequest r) => r.Headers["X-My-Header"]);

            using var host = TestHost.Run(app);

            var result = await SendAsync(host, (w) =>
            {
                w.WriteLine(" GET  /some-path/  HTTP/1.0");
                w.WriteLine(" Host : 127.0.0.1 ");
                w.WriteLine("    X-My-Header: abc     ");
                w.WriteLine();
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

        #endregion

        #region Helpers

        private static async ValueTask TestAsync(string request, string assertion)
        {
            using var host = TestHost.Run(Layout.Create());

            var result = await SendAsync(host, (w) =>
            {
                w.Write(request);
                w.WriteLine();
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

            using var writer = new StreamWriter(stream, leaveOpen: true);

            sender(writer);

            writer.Flush();

            using var reader = new StreamReader(stream, leaveOpen: true);

            return await reader.ReadToEndAsync();
        }

        #endregion

    }

}
