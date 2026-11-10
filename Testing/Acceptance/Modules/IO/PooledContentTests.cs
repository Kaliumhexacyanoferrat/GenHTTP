using System.Buffers;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Formattable;
using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class PooledContentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomPooledContentRendersEachResponse(ServerEngine engine)
    {
        var counter = 0;

        var handler = Handler.From(r => r.Respond().Content(GreetingContent.Create(++counter)).Build());

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        for (var i = 1; i <= 3; i++)
        {
            using var response = await host.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual($"Hello #{i}", await response.GetContentAsync());
        }

        using var head = await host.GetResponseAsync(host.GetRequest(method: HttpMethod.Head));

        await head.AssertStatusAsync(HttpStatusCode.OK);

        using var afterHead = await host.GetResponseAsync();

        Assert.AreEqual("Hello #5", await afterHead.GetContentAsync());
    }

    [TestMethod]
    public void TestDisposingTwiceReturnsInstanceOnce()
    {
        var content = FormattableContent<int>.Rent(1);

        content.Dispose();
        content.Dispose();

        var first = FormattableContent<int>.Rent(2);
        var second = FormattableContent<int>.Rent(3);

        Assert.AreSame(content, first);
        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public void TestConstructedContentIsNotPooled()
    {
        var constructed = new FormattableContent<int>(1);

        constructed.Dispose();

        Assert.AreNotSame(constructed, FormattableContent<int>.Rent(2));
    }

    private sealed class GreetingContent : PooledContent<GreetingContent>, IResponseContent
    {
        private int _number;

        public ulong? Length => null;

        public ContentType? Type => ContentType.TextPlain;

        public ReadOnlyMemory<byte>? Encoding => null;

        public static GreetingContent Create(int number)
        {
            var content = FromPool(static () => new GreetingContent());

            content._number = number;

            return content;
        }

        public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_number);

        public ValueTask WriteAsync(IResponseSink sink)
        {
            sink.Writer.Write(System.Text.Encoding.UTF8.GetBytes($"Hello #{_number}"));
            return ValueTask.CompletedTask;
        }

        protected override void Reset() => _number = 0;
    }

}
