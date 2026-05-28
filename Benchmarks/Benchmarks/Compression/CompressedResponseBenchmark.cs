using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;

namespace GenHTTP.Benchmarks.Benchmarks.Compression;

[MemoryDiagnoser]
public class CompressedResponseBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkCompressedResponse() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var tree = ResourceTree.FromDirectory("./Resources/");

        var compression = CompressedContent.Default();

        var handler = Resources.From(tree)
                               .Add(compression);

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: br;q=1, gzip;q=0.8\r\n\r\n";

        return new(request, handler.Build());
    }

}
