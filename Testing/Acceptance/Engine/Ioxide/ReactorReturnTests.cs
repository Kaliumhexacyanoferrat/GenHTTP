using System.Buffers;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// A handler that awaits something other than ioxide comes back on the thread that completed it. The engine
/// returns to the reactor before it writes the response, since the state it keeps per thread lives there.
/// </summary>
[TestClass]
public sealed class ReactorReturnTests
{

    [IoxideTestMethod]
    public async Task TestHttp1ResponseIsWrittenOnTheReactor()
        => Assert.AreEqual("reactor", await ServeAsync(HttpVersion.Version11));

    [IoxideTestMethod]
    public async Task TestHttp2ResponseIsWrittenOnTheReactor()
        => Assert.AreEqual("reactor", await ServeAsync(HttpVersion.Version20));

    // Asks for exactly one version over cleartext, so h2 goes out as prior knowledge.
    private static async Task<string> ServeAsync(Version version)
    {
        var port = (ushort)TestHost.NextPort();

        var server = Host.Create().Handler(new LeavingHandler()).Bind(IPAddress.Loopback, port, HttpProtocols.Http1AndHttp2);

        await server.StartAsync();

        try
        {
            using var client = new HttpClient(new SocketsHttpHandler())
            {
                DefaultRequestVersion = version,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };

            using var response = await client.GetAsync($"http://localhost:{port}/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            return await response.Content.ReadAsStringAsync();
        }
        finally
        {
            await server.StopAsync();
        }
    }

    // Leaves the reactor before it answers, as a handler awaiting a database or the BCL's HttpClient does.
    private sealed class LeavingHandler : IHandler
    {

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            await Task.Delay(1);

            return request.Respond().Content(new WhereContent()).Build();
        }

    }

    // Writes which thread writes it: a reactor's, or another.
    private sealed class WhereContent : IResponseContent
    {

        public ulong? Length => null;

        public ContentType? Type => null;

        public ReadOnlyMemory<byte>? Encoding => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong?)null);

        public ValueTask WriteAsync(IResponseSink sink)
        {
            sink.Writer.Write(ioxide.Reactor.Current is not null ? "reactor"u8 : "elsewhere"u8);
            return ValueTask.CompletedTask;
        }

    }

}
