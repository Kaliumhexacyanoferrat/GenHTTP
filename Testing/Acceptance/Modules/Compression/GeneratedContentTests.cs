using System.IO.Compression;
using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Functional;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

/// <summary>
/// Content generated per request (e.g. serialized JSON) has to arrive
/// completely, regardless of the compression algorithm negotiated.
/// </summary>
[TestClass]
public sealed class GeneratedContentTests
{

    public record Blob(string Data);

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeGeneratedContentIsComplete(ServerEngine engine)
    {
        var app = Inline.Create()
                        .Get(() => new Blob(new string('x', 80_000)));

        await using var runner = await TestHost.RunAsync(app, engine: engine);

        foreach (var algorithm in GetAlgorithms())
        {
            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", algorithm);

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual(algorithm, response.Content.Headers.ContentEncoding.First());

            var body = await ReadAsync(response, algorithm);

            Assert.AreEqual($"{{\"data\":\"{new string('x', 80_000)}\"}}", body, $"Content is incomplete for '{algorithm}'");
        }
    }

    private static List<string> GetAlgorithms()
    {
        var list = new List<string> { "gzip", "br" };

#if NET11_0_OR_GREATER
        list.Add("zstd");
#endif

        return list;
    }

    private static async Task<string> ReadAsync(HttpResponseMessage response, string algorithm)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();

        await using Stream decompressed = algorithm switch
        {
            "gzip" => new GZipStream(stream, CompressionMode.Decompress),
            "br" => new BrotliStream(stream, CompressionMode.Decompress),
#if NET11_0_OR_GREATER
            "zstd" => new ZstandardStream(stream, CompressionMode.Decompress),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };

        using var reader = new StreamReader(decompressed);

        return await reader.ReadToEndAsync();
    }

}
