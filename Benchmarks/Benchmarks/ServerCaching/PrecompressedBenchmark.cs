using System.IO.Compression;
using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.ServerCaching;

namespace GenHTTP.Benchmarks.Benchmarks.ServerCaching;

[MemoryDiagnoser]
public class PrecompressedBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkPrecompressed() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var compression = CompressedContent.Default()
                                           .Level(CompressionLevel.Optimal);

        var cache = ServerCache.TemporaryFiles()
                               .Invalidate(false);

        var handler = Assets.From("./Resources/") // serve static resources
                            .Add(compression) // compress them on-the-fly
                            .Add(cache); // cache the compressed results

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: br;q=1, gzip;q=0.8\r\n\r\n";

        return new(request, handler.Build());
    }

}
