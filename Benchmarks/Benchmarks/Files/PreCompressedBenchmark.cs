using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;

namespace GenHTTP.Benchmarks.Benchmarks.Files;

[MemoryDiagnoser]
public class PreCompressedBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkPreCompressedStaticFiles() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var handler = Assets.From("./Resources/")
                            .AllowPrecompressed(new BrotliAlgorithm())
                            .Add(CompressedContent.Default());

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: br;q=1, gzip;q=0.8\r\n\r\n";

        return new(request, handler.Build());
    }

}
