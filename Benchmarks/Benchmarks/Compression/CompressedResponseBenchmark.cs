using BenchmarkDotNet.Attributes;
using GenHTTP.Benchmarks.Infrastructure;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Files;

namespace GenHTTP.Benchmarks.Benchmarks.Compression;

[MemoryDiagnoser]
public class CompressedResponseBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkCompressedResponse() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var compression = CompressedContent.Default();

        var handler = Assets.From("./Resources/")
                            .Add(compression);

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: br;q=1, gzip;q=0.8\r\n\r\n";

        return new(request, handler.Build());
    }

}
