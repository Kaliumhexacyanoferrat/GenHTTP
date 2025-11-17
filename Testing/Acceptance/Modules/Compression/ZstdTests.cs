using System.IO.Compression;
using System.Linq;

using GenHTTP.Modules.Compression.Providers;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

[TestClass]
public class ZstdTests
{

    [TestMethod]
    public async Task TestCompressionLevels()
    {
        const string Payload = "Payload validated via zstd compression. ";

        foreach (var level in new[] { CompressionLevel.Fastest, CompressionLevel.Optimal, CompressionLevel.SmallestSize })
        {
            var compression = new CompressionConcernBuilder().Level(level)
                                                             .Add(new ZstdCompression());

            var app = Inline.Create()
                            .Get(() => string.Concat(Enumerable.Repeat(Payload, 20)))
                            .Add(compression);

            await using var runner = await TestHost.RunAsync(app, defaults: false);

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "zstd");

            using var response = await runner.GetResponseAsync(request);

            Assert.AreEqual("zstd", response.GetContentHeader("Content-Encoding"));
        }

    }

}
