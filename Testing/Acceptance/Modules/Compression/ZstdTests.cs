using System.IO.Compression;

using GenHTTP.Modules.Compression.Providers;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Compression;

[TestClass]
public class ZstdTests
{

    [TestMethod]
    public async Task TestCompressionLevels()
    {
        foreach (var level in new[] { CompressionLevel.Fastest, CompressionLevel.Optimal, CompressionLevel.SmallestSize })
        {
            var compression = new CompressionConcernBuilder().Level(level)
                                                             .Add(new ZstdCompression());

            var app = Inline.Create()
                            .Get(() => "42")
                            .Add(compression);

            await using var runner = await TestHost.RunAsync(app, defaults: false);

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "zstd");

            using var response = await runner.GetResponseAsync(request);

            Assert.AreEqual("zstd", response.GetContentHeader("Content-Encoding"));
        }

    }

}
