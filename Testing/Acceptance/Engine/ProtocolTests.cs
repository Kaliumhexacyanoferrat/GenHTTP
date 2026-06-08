using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ProtocolTests
{

    /// <summary>
    /// As a client I can stream data to the server.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestPost(TestEngine engine)
    {
        var recorder = new ValueRecorder();

        const string str = "From client with ❤";

        await using var runner = await TestHost.RunAsync(recorder.Wrap(), engine: engine);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent(str);

        using var _ = await runner.GetResponseAsync(request);

        Assert.AreEqual(str, recorder.Value);
    }

    /// <summary>
    /// As a client I can submit large data.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestPutLarge(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new ContentLengthResponder().Wrap(), engine: engine);

        using var content = new MemoryStream();

        var random = new Random();

        var buffer = new byte[65536];

        for (var i = 0; i < 20; i++) // 1.3 MB
        {
            random.NextBytes(buffer);
            content.Write(buffer, 0, buffer.Length);
        }

        content.Seek(0, SeekOrigin.Begin);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Put;
        request.Content = new StreamContent(content);

        using var response = await runner.GetResponseAsync(request);

        var error = await response.GetContentAsync();
        
        Assert.AreEqual("1310720", error);
    }

    private class ValueRecorder : IHandler
    {

        public string? Value { get; private set; }

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var body = request.GetBody(HeaderAccess.Retain);

            if (body is not null)
            {
                using var reader = new StreamReader(body.AsStream());
                Value = reader.ReadToEnd();
            }

            return new ValueTask<IResponse?>(request.Respond().Build());
        }
    }

    private class ContentLengthResponder : IHandler
    {

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var total = 0;

            var body = request.GetBody(HeaderAccess.Release);

            if (body != null)
            {
                var stream = body.AsStream();

                var read = 0;

                var buffer = new byte[4096];

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    total += read;
                }
            }

            return request.Respond()
                          .Content(new GenHTTP.Modules.IO.Strings.StringContent(total.ToString()))
                          .Build();
        }

    }

}
